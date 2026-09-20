using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.GetJobs;

/// <summary>
/// Serves every public job surface with one query:
///   index.html featured carousel -> ?onlyFeatured=true&amp;pageSize=6
///   job-list-v*.html             -> ?categorySlug=...&amp;jobType=Remote&amp;search=...
/// Defaults to Active-only so drafts and closed jobs never leak onto public pages;
/// pass includeAllStatuses=true for an admin/employer view.
/// </summary>
public sealed record GetJobsQuery : PagedQuery, IRequest<Result<PagedResult<JobDto>>>
{
    public string? Search { get; init; }
    public string? CategorySlug { get; init; }
    public JobType? JobType { get; init; }
    public ExperienceLevel? ExperienceLevel { get; init; }
    public string? Location { get; init; }
    public bool OnlyFeatured { get; init; }
    public bool IncludeAllStatuses { get; init; }
    public string? SortBy { get; init; }
    public bool Descending { get; init; }
}
