namespace LinkedIn.Modules.Blogs.Contracts;

/// <summary>Exactly what one blog card on the frontend needs.</summary>
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

public sealed record BlogDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string? ImageUrl,
    string CategoryName,
    string CategorySlug,
    Guid AuthorId,
    bool IsFeatured,
    int ViewCount,
    DateTimeOffset? PublishedAt);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
}

/// <summary>Public surface other modules may call. Keep it tiny.</summary>
public interface IBlogsModuleApi
{
    Task<IReadOnlyList<BlogCardDto>> GetLatestAsync(int count, CancellationToken ct = default);
}
