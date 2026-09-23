using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace LinkedIn.Modules.Users.Features.Register;

internal sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _timeProvider;

    public RegisterHandler(UserManager<AppUser> userManager, IEmailSender emailSender, TimeProvider timeProvider)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
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

        // No frontend confirmation page exists yet - the dev email shows the raw
        // values needed to call POST /api/auth/confirm-email directly. Once a
        // frontend page exists, replace this with a real link to it instead.
        var body = $$"""
    <p>Welcome, {{user.FirstName}}.</p>
    <p>Confirm your email by calling <code>POST /api/auth/confirm-email</code> with:</p>
    <pre>{ "userId": "{{user.Id}}", "token": "{{encodedToken}}" }</pre>
    """;

        await _emailSender.SendAsync(user.Email!, "Confirm your email", body, cancellationToken);

        return Result.Success();
    }
}
