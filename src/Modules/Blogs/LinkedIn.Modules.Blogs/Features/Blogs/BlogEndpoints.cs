using LinkedIn.Modules.Blogs.Features.Blogs.Create;
using LinkedIn.Modules.Blogs.Features.Blogs.Delete;
using LinkedIn.Modules.Blogs.Features.Blogs.GetBySlug;
using LinkedIn.Modules.Blogs.Features.Blogs.GetList;
using LinkedIn.Modules.Blogs.Features.Blogs.Update;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Blogs.Features.Blogs;

public static class BlogEndpoints
{
    public static IEndpointRouteBuilder MapBlogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/blogs").WithTags("Blogs");

        // GET /api/blogs?page=1&pageSize=9&categorySlug=solution&search=design&onlyFeatured=true
        group.MapGet("/", async (
            ISender sender,
            int page = 1,
            int pageSize = 9,
            string? categorySlug = null,
            string? search = null,
            bool? onlyFeatured = null,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new GetBlogsQuery(page, pageSize, categorySlug, search, onlyFeatured), ct);
            return Results.Ok(result);
        })
        .WithName("GetBlogs")
        .AllowAnonymous();

        // GET /api/blogs/print-publishing-qui-visual-quis-layout-mockups
        group.MapGet("/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
        {
            var blog = await sender.Send(new GetBlogBySlugQuery(slug), ct);
            return blog is null ? Results.NotFound() : Results.Ok(blog);
        })
        .WithName("GetBlogBySlug")
        .AllowAnonymous();

        group.MapPost("/", async (CreateBlogCommand command, ISender sender, CancellationToken ct) =>
        {
            var id = await sender.Send(command, ct);
            return Results.Created($"/api/blogs/{id}", new { id });
        })
        .WithName("CreateBlog")
        .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateBlogCommand command, ISender sender, CancellationToken ct) =>
        {
            if (id != command.Id) return Results.BadRequest("Route id and body id do not match.");
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("UpdateBlog")
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteBlogCommand(id), ct);
            return Results.NoContent();
        })
        .WithName("DeleteBlog")
        .RequireAuthorization();

        return app;
    }
}
