using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>A candidate's bookmarked job - powers candidate-dashboard-saved-jobs.html.</summary>
public sealed class SavedJob : BaseEntity
{
    public long JobId { get; set; }
    public Job? Job { get; set; }

    public string CandidateUserId { get; set; } = string.Empty;
}