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

        RuleFor(c => c.Role)
            .NotEmpty()
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be either 'Candidate' or 'Employer'.");
    }
}
