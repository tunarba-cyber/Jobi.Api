using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Users.Features.Register;

internal sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _timeProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        UserManager<AppUser> userManager,
        IEmailSender emailSender,
        TimeProvider timeProvider,
        IConfiguration configuration,
        ILogger<RegisterHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result.Failure(UserErrors.EmailAlreadyRegistered);

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = Enum.Parse<UserRole>(request.Role, ignoreCase: true),
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var details = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure(UserErrors.RegistrationFailed(details));
        }

        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl is not configured.");

        var confirmationLink = $"{frontendBaseUrl.TrimEnd('/')}/Account/ConfirmEmail" +
            $"?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(encodedToken)}";

        var body = $"""
            <p>Welcome, {user.FirstName}.</p>
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm my email</a></p>
            """;

        // The user already exists at this point, so a slow or broken mail
        // server must not turn a successful signup into a 500.
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(8));
            await _emailSender.SendAsync(user.Email!, "Confirm your email", body, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Confirmation email to {Email} could not be sent.", user.Email);
        }

        return Result.Success();
    }
}