using LinkedIn.Modules.Blogs.Contracts;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Blogs.Features.Blogs.GetList;

public sealed record GetBlogsQuery(
    int Page = 1,
    int PageSize = 9,
    string? CategorySlug = null,
    string? Search = null,
    bool? OnlyFeatured = null) : IRequest<PagedResult<BlogCardDto>>;

internal sealed class GetBlogsQueryHandler : IRequestHandler<GetBlogsQuery, PagedResult<BlogCardDto>>
{
    private readonly BlogsDbContext _db;

    public GetBlogsQueryHandler(BlogsDbContext db) => _db = db;

    public async Task<PagedResult<BlogCardDto>> Handle(GetBlogsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 50 ? 9 : request.PageSize;

        var query = _db.Blogs
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.IsPublished);

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var categorySlug = request.CategorySlug.ToLowerInvariant();
            query = query.Where(b => b.Category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim()}%";
            // SQL Server's default collation is case-insensitive (CI), so plain LIKE is enough here.
            // If your DB collation is case-sensitive (CS), wrap both sides in .ToUpper() instead.
            query = query.Where(b => EF.Functions.Like(b.Title, term) || EF.Functions.Like(b.Summary, term));
        }

        if (request.OnlyFeatured is true)
            query = query.Where(b => b.IsFeatured);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.IsFeatured)
            .ThenByDescending(b => b.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlogCardDto(
                b.Id,
                b.Title,
                b.Slug,
                b.Summary,
                b.ImageUrl,
                b.Category.Name,
                b.Category.Slug,
                b.IsFeatured,
                b.PublishedAt))
            .ToListAsync(ct);

        return new PagedResult<BlogCardDto>(items, page, pageSize, totalCount);
    }
}
