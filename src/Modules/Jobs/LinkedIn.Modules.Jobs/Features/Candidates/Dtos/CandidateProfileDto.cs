using LinkedIn.Modules.Jobs.Domain.Enums;

namespace LinkedIn.Modules.Jobs.Features.Candidates.Dtos;

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