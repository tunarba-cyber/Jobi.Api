using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;

namespace LinkedIn.Modules.Jobs.Features.Home.Dtos;

/// <summary>
/// Everything index.html needs in one round trip: the stats counters, the
/// "Most Demanding Categories" cards, and the featured-jobs carousel.
/// </summary>
public sealed record HomeSummaryDto(
    int TotalActiveJobs,
    int TotalCategories,
    int TotalCompanies,
    IReadOnlyList<CategoryDto> FeaturedCategories,
    IReadOnlyList<JobDto> FeaturedJobs);
