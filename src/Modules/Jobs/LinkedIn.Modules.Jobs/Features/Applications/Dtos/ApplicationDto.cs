using LinkedIn.Modules.Jobs.Domain.Enums;


namespace LinkedIn.Modules.Jobs.Features.Applications.Dtos
{
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
}
