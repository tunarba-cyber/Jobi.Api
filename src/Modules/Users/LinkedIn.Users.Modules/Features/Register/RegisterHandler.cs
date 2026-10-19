using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Infrastructure;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkedIn.Modules.Users.Features.Register;

internal sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TimeProvider _timeProvider;
    private readonly ConfirmationEmailService _confirmation;

    public RegisterHandler(UserManager<AppUser> userManager, TimeProvider timeProvider, ConfirmationEmailService confirmation)
    { _userManager = userManager; _timeProvider = timeProvider; _confirmation = confirmation; }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            if (existing.EmailConfirmed) return Result.Failure(UserErrors.EmailAlreadyRegistered);

            // Registered earlier but never confirmed (for example, the email failed to send): send a new link.
            // The password typed this time is NOT applied. Use "Forgot password" if it was forgotten.
            await _confirmation.SendAsync(existing, cancellationToken);
            return Result.Success();
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = Enum.Parse<UserRole>(request.Role, ignoreCase: true),
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Result.Failure(UserErrors.RegistrationFailed(string.Join("; ", created.Errors.Select(e => e.Description))));

        await _confirmation.SendAsync(user, cancellationToken); // failure is logged; the user can click "send again"
        return Result.Success();
    }
}