namespace LinkedIn.Modules.Messaging.Domain.Entities;

public sealed class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; set; }
    public DateTimeOffset? ReadAtUtc { get; set; }
}
