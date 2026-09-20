using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using LinkedIn.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Categories.GetCategories;

internal sealed class GetCategoriesHandler
    : IRequestHandler<GetCategoriesQuery, Result<PagedResult<CategoryDto>>>
{
    private readonly JobsDbContext _db;

    public GetCategoriesHandler(JobsDbContext db) => _db = db;

    public async Task<Result<PagedResult<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim();

        // AsNoTracking: reads never need the change tracker. Pronia tracked every
        // read, which costs memory and time on list endpoints.
        var query = _db.Categories
            .AsNoTracking()
            .WhereIf(!request.IncludeInactive, c => c.IsActive)
            .WhereIf(request.OnlyFeatured, c => c.IsFeatured)
            .WhereIf(!string.IsNullOrWhiteSpace(search), c => c.Name.Contains(search!));

        query = ApplySort(query, request.SortBy, request.Descending);

        // Project BEFORE paging so SQL selects only the DTO columns.
        var projected = query.Select(CategoryMappings.ToDto);

        var page = await projected.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        return Result.Success(page);
    }

    private static IQueryable<Category> ApplySort(IQueryable<Category> query, string? sortBy, bool descending) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "name" => descending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "created" => descending ? query.OrderByDescending(c => c.CreatedAtUtc) : query.OrderBy(c => c.CreatedAtUtc),
            // Default matches the homepage: curated order, then alphabetical.
            _ => descending
                ? query.OrderByDescending(c => c.DisplayOrder).ThenByDescending(c => c.Name)
                : query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
        };
}
