using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Messaging.Domain.Errors;

public static class MessagingErrors
{
    public static Error ConversationNotFound(long id) =>
        Error.NotFound("Conversation.NotFound", $"No conversation was found with id {id}.");

    public static Error NotParticipant =>
        Error.Forbidden("Conversation.Forbidden", "You are not part of this conversation.");

    public static Error InvalidContent =>
        Error.Validation("Message.Invalid", "Message content must be between 1 and 4000 characters.");
}