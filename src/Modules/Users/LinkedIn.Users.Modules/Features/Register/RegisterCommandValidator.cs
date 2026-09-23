using FluentValidation;
using LinkedIn.Modules.Users.Domain.Enums;

namespace LinkedIn.Modules.Users.Features.Register;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        // Fast feedback here; UserManager's own password policy (configured in
        // UsersModule) is still the actual enforcement point.
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");

        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(100);

        // Admin is a real UserRole value, but deliberately NOT choosable here -
        // public registration must never be able to grant itself Admin. Admin
        // accounts get created out-of-band (seed data / a promoted user), never
        // through this endpoint.
        RuleFor(c => c.Role)
            .NotEmpty()
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed)
                          && parsed != UserRole.Admin)
            .WithMessage("Role must be either 'Candidate' or 'Employer'.");
    }
}
