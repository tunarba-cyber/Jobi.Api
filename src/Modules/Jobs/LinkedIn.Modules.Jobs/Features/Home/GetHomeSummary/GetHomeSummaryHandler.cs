using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Categories;
using LinkedIn.Modules.Jobs.Features.Home.Dtos;
using LinkedIn.Modules.Jobs.Features.Jobs;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Home.GetHomeSummary;

internal sealed class GetHomeSummaryHandler : IRequestHandler<GetHomeSummaryQuery, Result<HomeSummaryDto>>
{
    private readonly JobsDbContext _db;

    public GetHomeSummaryHandler(JobsDbContext db) => _db = db;

    public async Task<Result<HomeSummaryDto>> Handle(
        GetHomeSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // Sequential, not parallel: a single DbContext instance cannot run
        // concurrent queries. Each of these is a fast, indexed query on its own
        // (see the indexes in JobConfiguration/CategoryConfiguration), so this
        // is still a single cheap round trip worth of work for the homepage.
        var totalActiveJobs = await _db.Jobs
            .AsNoTracking()
            .CountAsync(j => j.Status == JobStatus.Active, cancellationToken);

        var totalCategories = await _db.Categories
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        // Now that Company is a real entity, this is a direct count instead of
        // the old "distinct company name among active jobs" stand-in - counts
        // every registered company, whether or not they have posted a job yet.
        var totalCompanies = await _db.Companies
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var featuredCategories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsFeatured)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Take(request.FeaturedCategoriesCount)
            .Select(CategoryMappings.ToDto)
            .ToListAsync(cancellationToken);

        var featuredJobs = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Active && j.IsFeatured)
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(request.FeaturedJobsCount)
            .Select(JobMappings.ToDto)
            .ToListAsync(cancellationToken);

        var summary = new HomeSummaryDto(
            totalActiveJobs,
            totalCategories,
            totalCompanies,
            featuredCategories,
            featuredJobs);

        return Result.Success(summary);
    }
}
