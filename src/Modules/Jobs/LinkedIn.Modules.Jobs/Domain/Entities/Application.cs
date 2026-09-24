using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>
/// A candidate's application to one job. JobId is a REAL EF foreign key (same
/// DbContext as Job/Company), but CandidateUserId is a plain string - AppUser
/// lives in the Users module's own DbContext, so there's no live join possible
/// there regardless. CandidateName/Email are snapshotted at apply-time for the
/// same reason GetJobsHandler denormalizes Company.Name onto Job.
/// </summary>
public sealed class Application : BaseEntity
{
    public long JobId { get; set; }
    public Job? Job { get; set; }

    public string CandidateUserId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;

    public string? ResumeUrl { get; set; }
    public string? CoverLetter { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
}