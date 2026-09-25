namespace LinkedIn.MVC.Models.Api;

/// <summary>Matches HomeSummaryDto from GET /api/home/summary.</summary>
public sealed record HomeSummaryDto(
    int TotalActiveJobs,
    int TotalCategories,
    int TotalCompanies,
    IReadOnlyList<CategoryDto> FeaturedCategories,
    IReadOnlyList<JobDto> FeaturedJobs)
{
    public static HomeSummaryDto Empty { get; } =
        new(0, 0, 0, Array.Empty<CategoryDto>(), Array.Empty<JobDto>());
}
