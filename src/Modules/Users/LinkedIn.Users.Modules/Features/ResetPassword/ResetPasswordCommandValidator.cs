using FluentValidation;

namespace LinkedIn.Modules.Users.Features.ResetPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Token).NotEmpty();
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(8);
    }
}
