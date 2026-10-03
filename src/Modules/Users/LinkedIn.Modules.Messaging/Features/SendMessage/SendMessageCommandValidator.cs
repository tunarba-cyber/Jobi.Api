using FluentValidation;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

internal sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(c => c.RecipientUserId).NotEmpty();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(4000);
    }
}