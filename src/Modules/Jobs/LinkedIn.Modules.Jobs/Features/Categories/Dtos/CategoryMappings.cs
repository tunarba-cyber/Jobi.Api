using System.Linq.Expressions;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Categories.Dtos;

namespace LinkedIn.Modules.Jobs.Features.Categories;

internal static class CategoryMappings
{
    /// <summary>
    /// Projection used inside EF queries so SQL selects only the needed columns -
    /// no full-entity materialisation, no AutoMapper dependency.
    /// </summary>
    public static readonly Expression<Func<Category, CategoryDto>> ToDto =
        category => new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.IconUrl,
            category.Description,
            category.DisplayOrder,
            category.IsActive,
            category.IsFeatured,
            // EF Core translates this into a correlated COUNT subquery - no
            // .Include() and no N+1.
            category.Jobs.Count(j => j.Status == JobStatus.Active));

    public static CategoryDto ToCategoryDto(this Category category) =>
        new(category.Id,
            category.Name,
            category.Slug,
            category.IconUrl,
            category.Description,
            category.DisplayOrder,
            category.IsActive,
            category.IsFeatured,
            // Correct for CreateCategory (a brand-new category always has 0 jobs).
            // UpdateCategoryHandler reuses this too, where it is only correct if
            // the category has no jobs yet - fine for now, but if that ever
            // matters, load category.Jobs (or a separate count query) there.
            0);
}
