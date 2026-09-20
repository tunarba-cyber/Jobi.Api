namespace LinkedIn.Modules.Jobs.Features.Categories.Dtos;

/// <summary>
/// Exactly what a "Most Demanding Categories" card on index.html needs.
/// Entities are never returned directly from endpoints - that would leak audit
/// columns and create accidental coupling to the database schema.
/// </summary>
public sealed record CategoryDto(
    long Id,
    string Name,
    string Slug,
    string? IconUrl,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    bool IsFeatured,
    int JobCount);
