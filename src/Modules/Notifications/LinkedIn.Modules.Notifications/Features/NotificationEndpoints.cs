using System.Security.Claims;
using LinkedIn.Modules.Notifications.Infrastructure;
using LinkedIn.Shared.Abstractions.Paging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Notifications.Features;

public sealed record NotificationDto(long Id, string Type, string Title, string? Body, string? Link, bool IsRead, DateTimeOffset CreatedAtUtc);
public sealed record UnreadCountDto(int Count);

internal static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();
        g.MapGet("/", GetMine);
        g.MapGet("/unread-count", UnreadCount);
        g.MapPut("/{id:long}/read", MarkRead);
        g.MapPut("/read-all", MarkAllRead);
    }

    private static string Uid(ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private static async Task<IResult> GetMine(ClaimsPrincipal user, NotificationsDbContext db, CancellationToken ct, int page = 1, int pageSize = 20)
    {
        var uid = Uid(user);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Notifications.AsNoTracking().Where(n => n.UserId == uid);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Title, n.Body, n.Link, n.IsRead, n.CreatedAtUtc))
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<NotificationDto>(items, page, pageSize, total));
    }

    private static async Task<IResult> UnreadCount(ClaimsPrincipal user, NotificationsDbContext db, CancellationToken ct)
    {
        var uid = Uid(user);
        return Results.Ok(new UnreadCountDto(await db.Notifications.CountAsync(n => n.UserId == uid && !n.IsRead, ct)));
    }

    private static async Task<IResult> MarkRead(long id, ClaimsPrincipal user, NotificationsDbContext db, CancellationToken ct)
    {
        var uid = Uid(user);
        await db.Notifications.Where(n => n.Id == id && n.UserId == uid)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> MarkAllRead(ClaimsPrincipal user, NotificationsDbContext db, CancellationToken ct)
    {
        var uid = Uid(user);
        await db.Notifications.Where(n => n.UserId == uid && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        return Results.NoContent();
    }
}