using LinkedIn.Modules.Blogs.Domain.Entities;
using LinkedIn.Modules.Blogs.Infrastructure;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Blogs.Features.Blogs;

public static class BlogCategoryEndpoints
{
    public static IEndpointRouteBuilder MapBlogCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/blog-categories").WithTags("Blog categories");

        group.MapGet("/", async (BlogsDbContext db, CancellationToken ct) =>
        {
            var categories = await db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name, c.Slug, Count = c.Blogs.Count(b => b.IsPublished) })
                .ToListAsync(ct);

            return Results.Ok(categories);
        })
        .AllowAnonymous();

        group.MapPost("/", async (CreateCategoryRequest request, BlogsDbContext db, CancellationToken ct) =>
        {
            var slug = SlugGenerator.Generate(request.Name);
            if (await db.Categories.AnyAsync(c => c.Slug == slug, ct))
                return Results.Conflict($"Category '{request.Name}' already exists.");

            var category = BlogCategory.Create(request.Name, slug);
            db.Categories.Add(category);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/blog-categories/{category.Id}", new { category.Id, category.Name, category.Slug });
        })
        .RequireAuthorization();

        return app;
    }

    public sealed record CreateCategoryRequest(string Name);
}
