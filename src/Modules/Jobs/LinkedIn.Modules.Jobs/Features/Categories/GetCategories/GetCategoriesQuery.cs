using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Categories.GetCategories;

/// <summary>
/// Serves two callers at once:
///   index.html cards  -> ?onlyFeatured=true&amp;pageSize=6
///   admin list        -> ?search=...&amp;page=2&amp;includeInactive=true
/// </summary>
public sealed record GetCategoriesQuery : PagedQuery, IRequest<Result<PagedResult<CategoryDto>>>
{
    public string? Search { get; init; }
    public bool OnlyFeatured { get; init; }
    public bool IncludeInactive { get; init; }
    public string? SortBy { get; init; }
    public bool Descending { get; init; }
}
