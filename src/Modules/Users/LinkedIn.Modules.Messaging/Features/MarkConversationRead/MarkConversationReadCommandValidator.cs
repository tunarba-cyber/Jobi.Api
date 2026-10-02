using FluentValidation;

namespace LinkedIn.Modules.Messaging.Features.MarkConversationRead;

internal sealed class MarkConversationReadCommandValidator : AbstractValidator<MarkConversationReadCommand>
{
    public MarkConversationReadCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.ConversationId).NotEmpty();
    }
}
