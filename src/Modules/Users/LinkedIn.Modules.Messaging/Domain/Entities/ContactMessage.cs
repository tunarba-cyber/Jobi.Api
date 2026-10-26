namespace LinkedIn.Modules.Messaging.Domain.Entities;

public sealed class ContactMessage
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string Topic { get; set; } = "General";
    public string? Reference { get; set; }   // a site path such as /Posts/Details/5
}