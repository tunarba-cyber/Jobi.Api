namespace LinkedIn.Modules.Messaging.Domain.Entities;

public sealed class Conversation
{
    public long Id { get; set; }
    public string UserAId { get; set; } = string.Empty;
    public string UserBId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}