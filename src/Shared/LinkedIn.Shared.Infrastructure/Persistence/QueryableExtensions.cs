using LinkedIn.Shared.Abstractions.Paging;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Shared.Infrastructure.Persistence;

public static class QueryableExtensions
{
    /// <summary>Executes count + page in one place so no handler reinvents pagination.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            return PagedResult<T>.Empty(page, pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }

    /// <summary>Applies a filter only when the condition holds - keeps query building readable.</summary>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
        condition ? query.Where(predicate) : query;
}
