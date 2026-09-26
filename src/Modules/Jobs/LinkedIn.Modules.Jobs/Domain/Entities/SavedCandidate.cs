using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>An employer's bookmarked candidate - powers employer-dashboard-saved-candidate.html.</summary>
public sealed class SavedCandidate : BaseEntity
{
    public long CandidateProfileId { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }

    public string EmployerUserId { get; set; } = string.Empty;
}