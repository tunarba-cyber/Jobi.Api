namespace LinkedIn.MVC.Models.Api;

public sealed record BlogListItemDto(Guid Id, string Title, string Slug, string? CategoryName, bool IsFeatured, bool IsPublished, DateTimeOffset CreatedAtUtc);
public sealed record BlogDto(Guid Id, string Title, string Slug, string Content, Guid CategoryId, bool IsFeatured, bool IsPublished);
public sealed record BlogCardDto(Guid Id, string Title, string Slug, string? Summary, string? ImageUrl, string? CategoryName, DateTimeOffset CreatedAtUtc, int ViewCount = 0);
public sealed record BlogDetailsDto(Guid Id, string Title, string Slug, string Content, string? Summary, string? ImageUrl, string? CategoryName, string? CategorySlug, DateTimeOffset PublishedAt, int ViewCount);
public sealed record BlogCategoryDto(Guid Id, string Name, string Slug, int Count);
public sealed record CreateBlogRequest(string Title, string? Slug, string Content, Guid CategoryId, bool IsFeatured, bool IsPublished);
public sealed record UpdateBlogRequest(Guid Id, string Title, string? Slug, string Content, Guid CategoryId, bool IsFeatured, bool IsPublished);
public sealed record CreatedIdResponse(Guid Id);
