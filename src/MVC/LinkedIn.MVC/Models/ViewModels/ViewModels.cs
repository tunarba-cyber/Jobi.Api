using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Services;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class HomeIndexViewModel
{
    public required HomeSummaryDto Summary { get; init; }

    /// <summary>Full category list for the hero search dropdown.</summary>
    public IReadOnlyList<CategoryDto> AllCategories { get; init; } = Array.Empty<CategoryDto>();

    public IReadOnlyList<BlogCardDto> LatestBlogs { get; init; } = Array.Empty<BlogCardDto>();
}

public sealed class JobListViewModel
{
    public required PagedResult<JobDto> Jobs { get; init; }
    public required JobSearchRequest Filters { get; init; }
    public IReadOnlyList<CategoryDto> Categories { get; init; } = Array.Empty<CategoryDto>();

    public bool IsEmpty => Jobs.Items.Count == 0;

    /// <summary>
    /// Rebuilds the current query string with one value swapped - used by the
    /// pagination links and the sort dropdown so filters survive navigation.
    /// </summary>
    public IDictionary<string, string?> RouteValuesFor(int page) => new Dictionary<string, string?>
    {
        ["search"] = Filters.Search,
        ["categorySlug"] = Filters.CategorySlug,
        ["jobType"] = Filters.JobType?.ToString(),
        ["experienceLevel"] = Filters.ExperienceLevel?.ToString(),
        ["location"] = Filters.Location,
        ["sortBy"] = Filters.SortBy,
        ["descending"] = Filters.Descending ? "true" : null,
        ["page"] = page.ToString()
    };
}

public sealed class JobDetailsViewModel
{
    public required JobDto Job { get; init; }

    /// <summary>Other jobs in the same category, current one excluded.</summary>
    public IReadOnlyList<JobDto> RelatedJobs { get; init; } = Array.Empty<JobDto>();
}

public sealed class BlogListViewModel
{
    public required PagedResult<BlogCardDto> Blogs { get; init; }
    public IReadOnlyList<BlogCategoryDto> Categories { get; init; } = Array.Empty<BlogCategoryDto>();
    public string? CategorySlug { get; init; }
    public string? Search { get; init; }
}
