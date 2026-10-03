namespace LinkedIn.Modules.Messaging.Domain.Entities;

public sealed class Message
{
    public long Id { get; set; }
    public long ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset SentAtUtc { get; set; }
}