namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>Remembers that an alert already notified about a job, so it never notifies twice.</summary>
public sealed class JobAlertMatch
{
    public long Id { get; set; }
    public long AlertId { get; set; }
    public long JobId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}