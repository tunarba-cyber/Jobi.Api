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

namespace LinkedIn.Modules.Users.Features.Register;

internal sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _timeProvider;
    private readonly IConfiguration _configuration;

    public RegisterHandler(UserManager<AppUser> userManager, IEmailSender emailSender, TimeProvider timeProvider, IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
        _configuration = configuration;
         
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

        // UserManager hashes the password and enforces the policy configured
        // in UsersModule (min length, digit/upper/lower requirements, etc.).
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

        await _emailSender.SendAsync(user.Email!, "Confirm your email", body, cancellationToken);

        return Result.Success();
    }
}
