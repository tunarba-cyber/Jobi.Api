using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>
/// A candidate's saved search criteria - powers candidate-dashboard-job-alert.html.
/// Stores what to match against; no background matching/notification job exists
/// yet, so this is currently just persisted preferences, not an active alert.
/// </summary>
public sealed class JobAlert : BaseEntity
{
    public string CandidateUserId { get; set; } = string.Empty;

    public string? Keyword { get; set; }
    public long? CategoryId { get; set; }
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;
}