using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Candidates.SearchCandidates;

/// <summary>Powers candidates-v1..v4.html - the public, browsable directory.</summary>
public sealed record SearchCandidatesQuery : PagedQuery, IRequest<Result<PagedResult<CandidateProfileDto>>>
{
    public string? Search { get; init; }
    public string? Location { get; init; }
    public ExperienceLevel? ExperienceLevel { get; init; }
}