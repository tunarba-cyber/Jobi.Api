using System.Net;
using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Users.Features.ForgotPassword;

internal sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(UserManager<AppUser> userManager, IEmailSender emailSender, IConfiguration config, ILogger<ForgotPasswordHandler> logger)
    { _userManager = userManager; _emailSender = emailSender; _config = config; _logger = logger; }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        // Always "success" so the caller can't tell whether an account exists.
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Result.Success();

        var baseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("Frontend:BaseUrl is not configured; reset email not sent.");
            return Result.Success();
        }

        var raw = await _userManager.GeneratePasswordResetTokenAsync(user);
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(raw));
        var link = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        var html = $"<p>We received a request to reset your password.</p>" +
                   $"<p><a href=\"{WebWebEncode(link)}\">Choose a new password</a></p>" +
                   "<p>If you didn't ask for this, you can ignore this email.</p>";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(8));
            await _emailSender.SendAsync(user.Email!, "Reset your password", html, cts.Token);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Password reset email to {Email} could not be sent.", user.Email);
        }

        return Result.Success();
    }

    private static string WebWebEncode(string s) => WebUtility.HtmlEncode(s);
}