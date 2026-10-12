using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

public sealed class Interview : BaseEntity
{
    public long ApplicationId { get; set; }
    public Application? Application { get; set; }
    public DateTimeOffset ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public InterviewMode Mode { get; set; }
    public string LocationOrLink { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
}