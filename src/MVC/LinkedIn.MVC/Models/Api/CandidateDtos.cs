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
public sealed record CandidateProfileDto(
    long Id,
    string Slug,
    string FullName,
    string Headline,
    string? Bio,
    string? Location,
    string? PhotoUrl,
    string? ResumeUrl,
    string? Skills,
    ExperienceLevel ExperienceLevel,
    bool IsAvailableForWork);

public sealed record UpsertCandidateProfileRequest(
    string FullName,
    string Headline,
    string? Bio,
    string? Location,
    string? PhotoUrl,
    string? ResumeUrl,
    string? Skills,
    int ExperienceLevel,          // int, not the enum - same JSON-enum trap as JobWriteRequest
    bool IsAvailableForWork);
public sealed record CandidateSearchRequest
{
    public string? Search { get; init; }
    public string? Location { get; init; }
    public ExperienceLevel? ExperienceLevel { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}