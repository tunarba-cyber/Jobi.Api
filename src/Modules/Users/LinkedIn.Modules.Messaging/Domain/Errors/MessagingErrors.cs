using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Messaging.Domain.Errors;

public static class MessagingErrors
{
    public static readonly Error CannotMessageSelf =
        Error.Validation("Messaging.CannotMessageSelf", "You cannot send a message to yourself.");

    public static readonly Error RecipientNotFound =
        Error.NotFound("Messaging.RecipientNotFound", "The recipient does not exist.");

    // Same error for "doesn't exist" and "not yours" so ids can't be probed.
    public static readonly Error ConversationNotFound =
        Error.NotFound("Messaging.ConversationNotFound", "Conversation was not found.");
}