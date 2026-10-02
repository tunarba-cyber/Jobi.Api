using FluentValidation;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

internal sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(c => c.SenderId).NotEmpty();
        RuleFor(c => c.RecipientId).NotEmpty().MaximumLength(64);
        RuleFor(c => c.Body).NotEmpty().MaximumLength(4000);
    }
}
