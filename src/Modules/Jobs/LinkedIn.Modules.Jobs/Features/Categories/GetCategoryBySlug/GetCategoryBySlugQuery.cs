using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Categories.GetCategoryBySlug;

/// <summary>
/// Lookup by slug, not id, so public URLs stay readable and stable:
/// /job-grid-v1.html?category=ui-ux-design
/// </summary>
public sealed record GetCategoryBySlugQuery(string Slug) : IRequest<Result<CategoryDto>>;
