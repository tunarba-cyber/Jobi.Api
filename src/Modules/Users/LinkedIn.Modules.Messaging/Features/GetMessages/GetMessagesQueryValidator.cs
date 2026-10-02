using FluentValidation;

namespace LinkedIn.Modules.Messaging.Features.GetMessages;

internal sealed class GetMessagesQueryValidator : AbstractValidator<GetMessagesQuery>
{
    public GetMessagesQueryValidator()
    {
        RuleFor(q => q.UserId).NotEmpty();
        RuleFor(q => q.ConversationId).NotEmpty();
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}
