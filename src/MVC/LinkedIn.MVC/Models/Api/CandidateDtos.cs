namespace LinkedIn.MVC.Models.Api;

public enum ApplicationStatus { Applied, Reviewed, Shortlisted, Rejected, Hired }

public sealed record ApplicationDto(
    long Id,
    long JobId,
    string JobTitle,
    long CompanyId,
    string CompanyName,
    string CandidateUserId,
    string CandidateName,
    string CandidateEmail,
    string? ResumeUrl,
    string? CoverLetter,
    ApplicationStatus Status,
    DateTimeOffset AppliedAtUtc);

public sealed record ApplyRequest(long JobId, string? ResumeUrl, string? CoverLetter);

public sealed record SavedJobDto(
    long Id,
    long JobId,
    string JobTitle,
    string JobSlug,
    string CompanyName,
    DateTimeOffset SavedAtUtc);