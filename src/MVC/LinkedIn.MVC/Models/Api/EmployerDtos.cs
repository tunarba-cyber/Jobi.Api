namespace LinkedIn.MVC.Models.Api;

public sealed record CompanyDto(
    long Id,
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    string? LogoUrl,
    DateTimeOffset CreatedAtUtc);
public sealed record ApplicantDto(
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

public sealed record CreateCompanyRequest(string Name, string? Slug, string? Description, string? WebsiteUrl, string? LogoUrl);
public sealed record UpdateCompanyRequest(string Name, string? Description, string? WebsiteUrl, string? LogoUrl);

/// <summary>
/// Used for both create and update. The three enums are plain ints ON PURPOSE:
/// the API has no JsonStringEnumConverter, and MVC's JsonOptions would otherwise
/// serialize them as strings ("FullTime") and the API would answer 400.
/// Status is ignored by POST (CreateJobCommand has no such field).
/// </summary>
public sealed record JobWriteRequest(
    string Title,
    string? Slug,
    string Description,
    long CategoryId,
    string Location,
    int JobType,
    int ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    int VacancyCount,
    DateTimeOffset? ApplicationDeadline,
    int Status,
    bool IsFeatured);
public sealed record SavedCandidateDto(
    long Id,
    long CandidateProfileId,
    string CandidateName,
    string Headline,
    DateTimeOffset SavedAtUtc);