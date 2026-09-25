namespace LinkedIn.MVC.Models.Api;

/// <summary>
/// Client-side mirror of the API's pagination envelope. TotalPages/HasNextPage
/// are computed here rather than deserialised - the API exposes them as
/// read-only computed properties, so they are present in the JSON but there is
/// no reason to trust them over the raw numbers.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Empty(int page = 1, int pageSize = 10) =>
        new() { Items = Array.Empty<T>(), Page = page, PageSize = pageSize, TotalCount = 0 };
}
