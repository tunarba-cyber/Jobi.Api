using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using LinkedIn.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Jobs.GetJobs;

internal sealed class GetJobsHandler : IRequestHandler<GetJobsQuery, Result<PagedResult<JobDto>>>
{
    private readonly JobsDbContext _db;

    public GetJobsHandler(JobsDbContext db) => _db = db;

    public async Task<Result<PagedResult<JobDto>>> Handle(
        GetJobsQuery request,
        CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim();
        var categorySlug = request.CategorySlug?.Trim().ToLowerInvariant();
        var location = request.Location?.Trim();

        var query = _db.Jobs
            .AsNoTracking()
            .WhereIf(!request.IncludeAllStatuses, j => j.Status == JobStatus.Active)
            .WhereIf(request.OnlyFeatured, j => j.IsFeatured)
            .WhereIf(!string.IsNullOrWhiteSpace(categorySlug), j => j.Category!.Slug == categorySlug)
            .WhereIf(request.JobType.HasValue, j => j.JobType == request.JobType)
            .WhereIf(request.ExperienceLevel.HasValue, j => j.ExperienceLevel == request.ExperienceLevel)
            .WhereIf(!string.IsNullOrWhiteSpace(location), j => j.Location.Contains(location!))
            .WhereIf(!string.IsNullOrWhiteSpace(search),
                j => j.Title.Contains(search!) || j.Company!.Name.Contains(search!));

        query = ApplySort(query, request.SortBy, request.Descending);

        // Project BEFORE paging so SQL selects only the DTO columns.
        var projected = query.Select(JobMappings.ToDto);

        var page = await projected.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(page);
    }

    private static IQueryable<Job> ApplySort(IQueryable<Job> query, string? sortBy, bool descending) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "title" => descending ? query.OrderByDescending(j => j.Title) : query.OrderBy(j => j.Title),
            "deadline" => descending
                ? query.OrderByDescending(j => j.ApplicationDeadline)
                : query.OrderBy(j => j.ApplicationDeadline),
            "salary" => descending ? query.OrderByDescending(j => j.SalaryMax) : query.OrderBy(j => j.SalaryMax),
            "created" => descending ? query.OrderByDescending(j => j.CreatedAtUtc) : query.OrderBy(j => j.CreatedAtUtc),
            // No sortBy given: newest-first, matching every job-list template's default order.
            _ => query.OrderByDescending(j => j.CreatedAtUtc)
        };
}
