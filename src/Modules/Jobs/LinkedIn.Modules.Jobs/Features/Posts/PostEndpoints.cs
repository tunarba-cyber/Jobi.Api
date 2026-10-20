using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LinkedIn.Modules.Jobs.Features.Posts;

public sealed record PostDto(long Id, string AuthorId, string AuthorName, string AuthorRole, string Caption, string? ImageUrl,
    int Likes, int Comments, bool LikedByMe, bool CanDelete, DateTimeOffset CreatedAtUtc);
public sealed record CommentDto(long Id, string UserId, string UserName, string Text, bool CanDelete, DateTimeOffset CreatedAtUtc);
public sealed record CreatePostRequest(string? Caption, string? ImageUrl);
public sealed record CreateCommentRequest(string? Text);

internal static class PostEndpoints
{
    private static readonly Action<AuthorizationPolicyBuilder> Posters = p => p.RequireRole(AppRoles.Candidate, AppRoles.Employer);

    public static void MapPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/posts").WithTags("Posts").RequireAuthorization();
        g.MapGet("/", Feed);
        g.MapGet("/{id:long}", GetOne);
        g.MapPost("/", Create).RequireAuthorization(Posters);
        g.MapDelete("/{id:long}", Delete);
        g.MapPost("/{id:long}/like", Like).RequireAuthorization(Posters);
        g.MapDelete("/{id:long}/like", Unlike);
        g.MapGet("/{id:long}/comments", Comments);
        g.MapPost("/{id:long}/comments", AddComment).RequireAuthorization(Posters);
        g.MapDelete("/comments/{commentId:long}", DeleteComment);
    }

    private static string Uid(ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private static IResult Bad(string detail) => Results.Problem(title: "Post.Invalid", detail: detail, statusCode: 400);

    private static async Task<List<PostDto>> ToDtos(IQueryable<Post> page, string me, bool admin, JobsDbContext db, IUserDirectory dir, CancellationToken ct)
    {
        var rows = await page.Select(p => new
        {
            p.Id,
            p.AuthorUserId,
            p.Caption,
            p.ImageUrl,
            p.CreatedAtUtc,
            Likes = db.PostLikes.Count(l => l.PostId == p.Id),
            Comments = db.PostComments.Count(c => c.PostId == p.Id),
            Liked = db.PostLikes.Any(l => l.PostId == p.Id && l.UserId == me)
        }).ToListAsync(ct);

        var cards = await dir.GetCardsAsync(rows.Select(r => r.AuthorUserId), ct);
        return rows.Select(r =>
        {
            cards.TryGetValue(r.AuthorUserId, out var c);
            return new PostDto(r.Id, r.AuthorUserId, c is null ? "Member" : $"{c.FirstName} {c.LastName}", c?.Role ?? "",
                r.Caption, r.ImageUrl, r.Likes, r.Comments, r.Liked, admin || r.AuthorUserId == me, r.CreatedAtUtc);
        }).ToList();
    }

    private static async Task<IResult> Feed(ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct,
        string? scope = null, string? authorId = null, int page = 1, int pageSize = 10)
    {
        var me = Uid(user);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 30);

        IQueryable<Post> q = db.Posts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(authorId)) q = q.Where(p => p.AuthorUserId == authorId);
        else if (scope == "following")
            q = q.Where(p => p.AuthorUserId == me || db.UserFollows.Any(f => f.FollowerUserId == me && f.FollowedUserId == p.AuthorUserId));

        var total = await q.CountAsync(ct);
        var paged = q.OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize);
        var items = await ToDtos(paged, me, user.IsInRole(AppRoles.Admin), db, dir, ct);
        return Results.Ok(new PagedResult<PostDto>(items, page, pageSize, total));
    }

    private static async Task<IResult> GetOne(long id, ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct)
    {
        var items = await ToDtos(db.Posts.AsNoTracking().Where(p => p.Id == id), Uid(user), user.IsInRole(AppRoles.Admin), db, dir, ct);
        return items.Count == 0 ? Results.NotFound() : Results.Ok(items[0]);
    }

    private static async Task<IResult> Create(CreatePostRequest r, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        var caption = r.Caption?.Trim() ?? "";

        if (caption.Length == 0 && string.IsNullOrWhiteSpace(r.ImageUrl)) return Bad("Write something or add a photo.");
        if (caption.Length > 2000) return Bad("A post can be at most 2000 characters.");
        if (r.ImageUrl is not null && (r.ImageUrl.Length > 300 || r.ImageUrl.Contains("..")
                || !r.ImageUrl.StartsWith("/uploads/posts/", StringComparison.Ordinal))) return Bad("Invalid image.");

        var since = DateTimeOffset.UtcNow.AddHours(-1);
        if (await db.Posts.CountAsync(p => p.AuthorUserId == me && p.CreatedAtUtc >= since, ct) >= 10)
            return Results.Problem(title: "Post.RateLimit", detail: "You're posting too fast. Try again later.", statusCode: 429);

        var post = new Post { AuthorUserId = me, Caption = caption, ImageUrl = r.ImageUrl };
        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/posts/{post.Id}", new { post.Id });
    }

    private static async Task<IResult> Delete(long id, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post is null) return Results.NoContent();
        if (post.AuthorUserId != Uid(user) && !user.IsInRole(AppRoles.Admin)) return Results.Forbid();

        db.Posts.Remove(post); // soft-deleted by interceptor
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Like(long id, ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, INotificationPublisher notify, CancellationToken ct)
    {
        var me = Uid(user);
        var author = await db.Posts.AsNoTracking().Where(p => p.Id == id).Select(p => p.AuthorUserId).FirstOrDefaultAsync(ct);
        if (author is null) return Results.NotFound();
        if (await db.PostLikes.AnyAsync(l => l.PostId == id && l.UserId == me, ct)) return Results.NoContent();

        db.PostLikes.Add(new PostLike { PostId = id, UserId = me });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Results.NoContent(); } // double-click race

        if (author != me)
        {
            var who = (await dir.GetSummariesAsync(new[] { me }, ct)).GetValueOrDefault(me);
            await notify.PublishAsync(author, NotificationTypes.Social, "New like", $"{who?.FirstName} {who?.LastName} liked your post.",
                $"/Posts/Details/{id}", groupKey: $"like-{id}", ct: ct);
        }
        return Results.NoContent();
    }

    private static async Task<IResult> Unlike(long id, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        var like = await db.PostLikes.FirstOrDefaultAsync(l => l.PostId == id && l.UserId == me, ct);
        if (like is not null) { db.PostLikes.Remove(like); await db.SaveChangesAsync(ct); }
        return Results.NoContent();
    }

    private static async Task<IResult> Comments(long id, ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct)
    {
        var me = Uid(user);
        var owner = await db.Posts.AsNoTracking().Where(p => p.Id == id).Select(p => p.AuthorUserId).FirstOrDefaultAsync(ct);
        if (owner is null) return Results.NotFound();

        var rows = await db.PostComments.AsNoTracking().Where(c => c.PostId == id).OrderBy(c => c.CreatedAtUtc).Take(200).ToListAsync(ct);
        var names = await dir.GetSummariesAsync(rows.Select(c => c.UserId), ct);
        var admin = user.IsInRole(AppRoles.Admin);

        return Results.Ok(rows.Select(c =>
        {
            names.TryGetValue(c.UserId, out var n);
            return new CommentDto(c.Id, c.UserId, n is null ? "Member" : $"{n.FirstName} {n.LastName}", c.Text,
                admin || c.UserId == me || owner == me, c.CreatedAtUtc);
        }).ToList());
    }

    private static async Task<IResult> AddComment(long id, CreateCommentRequest r, ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, INotificationPublisher notify, CancellationToken ct)
    {
        var me = Uid(user);
        var text = r.Text?.Trim() ?? "";
        if (text.Length == 0 || text.Length > 500) return Bad("A comment must be 1-500 characters.");

        var author = await db.Posts.AsNoTracking().Where(p => p.Id == id).Select(p => p.AuthorUserId).FirstOrDefaultAsync(ct);
        if (author is null) return Results.NotFound();

        var comment = new PostComment { PostId = id, UserId = me, Text = text };
        db.PostComments.Add(comment);
        await db.SaveChangesAsync(ct);

        var who = (await dir.GetSummariesAsync(new[] { me }, ct)).GetValueOrDefault(me);
        var name = $"{who?.FirstName} {who?.LastName}";
        if (author != me)
            await notify.PublishAsync(author, NotificationTypes.Social, "New comment", $"{name}: {(text.Length > 80 ? text[..80] + "…" : text)}",
                $"/Posts/Details/{id}", groupKey: $"comment-{id}", ct: ct);

        return Results.Created($"/api/posts/{id}/comments", new CommentDto(comment.Id, me, name, text, true, comment.CreatedAtUtc));
    }

    private static async Task<IResult> DeleteComment(long commentId, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        var c = await db.PostComments.FirstOrDefaultAsync(x => x.Id == commentId, ct);
        if (c is null) return Results.NoContent();

        var owner = await db.Posts.AsNoTracking().Where(p => p.Id == c.PostId).Select(p => p.AuthorUserId).FirstOrDefaultAsync(ct);
        if (c.UserId != me && owner != me && !user.IsInRole(AppRoles.Admin)) return Results.Forbid();

        db.PostComments.Remove(c);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}