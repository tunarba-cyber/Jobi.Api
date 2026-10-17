namespace LinkedIn.Shared.Abstractions.Contracts;

public static class NotificationTypes
{
    public const string NewApplication = "NewApplication";
    public const string ApplicationStatus = "ApplicationStatus";
    public const string Interview = "Interview";
    public const string Message = "Message";
    public const string Follow = "Follow";
    public const string JobMatch = "JobMatch";
}

public interface INotificationPublisher
{
    /// <summary>Never throws: a failed notification must not break the action that triggered it.
    /// If groupKey matches an unread notification for that user, it is updated instead of duplicated.</summary>
    Task PublishAsync(string userId, string type, string title, string? body, string? link,
        string? groupKey = null, CancellationToken ct = default);
}