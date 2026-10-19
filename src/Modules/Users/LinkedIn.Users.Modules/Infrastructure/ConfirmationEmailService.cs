using System.Net;
using System.Security.Cryptography;
using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Users.Infrastructure;

/// <summary>
/// Confirmation links are an HMAC of the user id and email, so they never expire, can be reused,
/// and resending always produces the same working link.
/// </summary>
internal sealed class ConfirmationEmailService
{
    private const string Prefix = "v2.";
    private readonly byte[] _key;
    private readonly IConfiguration _config;
    private readonly IEmailSender _sender;
    private readonly ILogger<ConfirmationEmailService> _logger;

    public ConfirmationEmailService(IConfiguration config, IEmailSender sender, ILogger<ConfirmationEmailService> logger)
    {
        var secret = config[$"{JwtOptions.SectionName}:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        _key = Encoding.UTF8.GetBytes(secret + "|email-confirmation");
        _config = config; _sender = sender; _logger = logger;
    }

    private string Sign(AppUser u) =>
        Prefix + WebEncoders.Base64UrlEncode(
            HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{u.Id}:{u.Email?.ToLowerInvariant()}")));

    public bool Verify(AppUser u, string token) =>
        token.StartsWith(Prefix, StringComparison.Ordinal) &&
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(token), Encoding.UTF8.GetBytes(Sign(u)));

    /// <summary>Never throws. Returns false if the email couldn't be sent.</summary>
    public async Task<bool> SendAsync(AppUser user, CancellationToken ct)
    {
        var baseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("Frontend:BaseUrl is not configured; confirmation email not sent.");
            return false;
        }

        var link = $"{baseUrl}/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(Sign(user))}";
        var html = $"<p>Welcome, {WebUtility.HtmlEncode(user.FirstName)}.</p>" +
                   $"<p>Please confirm your email: <a href=\"{WebUtility.HtmlEncode(link)}\">Confirm my email</a></p>";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(8));
            await _sender.SendAsync(user.Email!, "Confirm your email", html, cts.Token);
            return true;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Confirmation email to {Email} could not be sent.", user.Email);
            return false;
        }
    }
}