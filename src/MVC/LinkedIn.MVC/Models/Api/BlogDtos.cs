namespace LinkedIn.MVC.Models.Api;

/// <summary>Matches BlogCardDto from GET /api/blogs.</summary>
public sealed record BlogCardDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string? ImageUrl,
    string CategoryName,
    string CategorySlug,
    bool IsFeatured,
    DateTimeOffset? PublishedAt);

/// <summary>Matches BlogDetailsDto from GET /api/blogs/{slug}.</summary>
public sealed record BlogDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string? ImageUrl,
    string CategoryName,
    string CategorySlug,
    string AuthorId,
    bool IsFeatured,
    int ViewCount,
    DateTimeOffset? PublishedAt);

/// <summary>
/// Matches an item from GET /api/blog-categories. Count is the number of
/// published posts in the category - the sidebar widget shows it.
/// </summary>
public sealed record BlogCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    int Count);
