using FluentValidation;

namespace LinkedIn.Modules.Users.Features.ConfirmEmail;

internal sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Token).NotEmpty();
    }
}
