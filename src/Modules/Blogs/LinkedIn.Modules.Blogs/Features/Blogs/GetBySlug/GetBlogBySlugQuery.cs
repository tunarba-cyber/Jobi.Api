using LinkedIn.Modules.Blogs.Contracts;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Blogs.Features.Blogs.GetBySlug;

public sealed record GetBlogBySlugQuery(string Slug) : IRequest<BlogDetailsDto?>;

internal sealed class GetBlogBySlugQueryHandler : IRequestHandler<GetBlogBySlugQuery, BlogDetailsDto?>
{
    private readonly BlogsDbContext _db;

    public GetBlogBySlugQueryHandler(BlogsDbContext db) => _db = db;

    public async Task<BlogDetailsDto?> Handle(GetBlogBySlugQuery request, CancellationToken ct)
    {
        var slug = request.Slug.ToLowerInvariant();

        var blog = await _db.Blogs
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished, ct);

        if (blog is null) return null;

        blog.IncreaseViewCount();
        await _db.SaveChangesAsync(ct);

        return new BlogDetailsDto(
            blog.Id,
            blog.Title,
            blog.Slug,
            blog.Summary,
            blog.Content,
            blog.ImageUrl,
            blog.Category.Name,
            blog.Category.Slug,
            blog.AuthorId,
            blog.IsFeatured,
            blog.ViewCount,
            blog.PublishedAt);
    }
}
