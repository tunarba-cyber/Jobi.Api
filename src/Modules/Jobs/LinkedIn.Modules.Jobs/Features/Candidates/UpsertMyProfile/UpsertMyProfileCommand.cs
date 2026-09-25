using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Candidates.UpsertMyProfile;

/// <summary>
/// One command handles both "create my profile" and "edit my profile" - if the
/// caller has no profile yet, one is created; if they do, it's updated. Simpler
/// than separate Create/Update commands for a 1-per-user resource.
/// </summary>
public sealed record UpsertMyProfileCommand(
    string RequestingUserId,
    string FullName,
    string Headline,
    string? Bio,
    string? Location,
    string? PhotoUrl,
    string? ResumeUrl,
    string? Skills,
    ExperienceLevel ExperienceLevel,
    bool IsAvailableForWork) : IRequest<Result<CandidateProfileDto>>;