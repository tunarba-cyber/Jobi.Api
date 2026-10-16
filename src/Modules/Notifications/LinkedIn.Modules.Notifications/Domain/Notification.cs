namespace LinkedIn.Modules.Notifications.Domain;

public sealed class Notification
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? Link { get; set; }          // an MVC path such as /Interviews/My
    public string? GroupKey { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}