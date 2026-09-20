namespace LinkedIn.Shared.Abstractions.Paging;

/// <summary>
/// Standard pagination envelope for every list endpoint, so the frontend can rely
/// on one shape. Pronia's GetAllProducts took page/pageSize but returned a bare
/// list, leaving the client unable to render pagination controls.
/// </summary>
public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Empty(int page, int pageSize) => new(Array.Empty<T>(), page, pageSize, 0);
}

/// <summary>Base for any query that pages. Values are clamped so a client cannot request 1,000,000 rows.</summary>
public abstract record PagedQuery
{
    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch
        {
            < 1 => 10,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }
}
