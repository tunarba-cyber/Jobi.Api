namespace LinkedIn.MVC.Models.Api;

/// <summary>Matches CategoryDto from GET /api/categories.</summary>
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
