using LinkedIn.Modules.Jobs.Features.Home.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Home.GetHomeSummary;

/// <summary>
/// Backs the whole homepage in one call instead of the frontend firing three or
/// four separate requests (categories, jobs, counters) on page load.
/// </summary>
public sealed record GetHomeSummaryQuery(int FeaturedCategoriesCount = 6, int FeaturedJobsCount = 6)
    : IRequest<Result<HomeSummaryDto>>;
