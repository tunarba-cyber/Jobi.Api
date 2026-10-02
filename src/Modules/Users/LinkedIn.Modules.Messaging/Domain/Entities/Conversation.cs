namespace LinkedIn.Modules.Messaging.Domain.Entities;

/// <summary>
/// One-to-one conversation. The two participant ids are stored in a canonical
/// (ordinal) order so the pair (A,B) and (B,A) always resolve to the same row,
/// which the unique index enforces. User ids are plain strings - no cross-module FK.
/// </summary>
public sealed class Conversation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserAId { get; private set; } = string.Empty;
    public string UserBId { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? LastMessageAtUtc { get; private set; }
    public string? LastMessagePreview { get; private set; }

    public List<Message> Messages { get; private set; } = [];

    private Conversation() { }

    public static (string A, string B) OrderPair(string first, string second) =>
        string.CompareOrdinal(first, second) <= 0 ? (first, second) : (second, first);

    public static Conversation Start(string first, string second, DateTimeOffset now)
    {
        var (a, b) = OrderPair(first, second);
        return new Conversation { UserAId = a, UserBId = b, CreatedAtUtc = now };
    }

    public bool Includes(string userId) => UserAId == userId || UserBId == userId;

    public string OtherParticipant(string userId) => UserAId == userId ? UserBId : UserAId;

    public Message AddMessage(string senderId, string body, DateTimeOffset now)
    {
        var message = new Message
        {
            ConversationId = Id,
            SenderId = senderId,
            Body = body,
            SentAtUtc = now
        };
        Messages.Add(message);

        LastMessageAtUtc = now;
        LastMessagePreview = body.Length <= 100 ? body : body[..100];
        return message;
    }
}
