using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Paging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features;

public sealed record ContactMessageDto(long Id, string Name, string Email, string? Subject, string Body,
    bool IsRead, DateTimeOffset CreatedAtUtc, string Topic, string? Reference);

internal static class AdminContactEndpoints
{
    public static void MapAdminContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/contact-messages").WithTags("AdminContact").RequireAuthorization("RequireAdmin");
        group.MapGet("/", GetAll);
        group.MapPut("/{id:long}/read", MarkRead);
        group.MapDelete("/{id:long}", Delete);
    }

    private static async Task<IResult> GetAll(MessagingDbContext db, CancellationToken ct, bool unreadOnly = false, int page = 1, int pageSize = 15, string? topic = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.ContactMessages.AsNoTracking()
            .Where(m => !unreadOnly || !m.IsRead)
            .Where(m => topic == null || (topic == "Reports" ? m.Topic.StartsWith("Report") : m.Topic == topic));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(m => m.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new ContactMessageDto(m.Id, m.Name, m.Email, m.Subject, m.Body, m.IsRead, m.CreatedAtUtc, m.Topic, m.Reference))
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<ContactMessageDto>(items, page, pageSize, total));
    }

    private static async Task<IResult> MarkRead(long id, MessagingDbContext db, CancellationToken ct)
    {
        var m = await db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return Results.NotFound();
        m.IsRead = true;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Delete(long id, MessagingDbContext db, CancellationToken ct)
    {
        var m = await db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is not null) { db.ContactMessages.Remove(m); await db.SaveChangesAsync(ct); }
        return Results.NoContent();
    }
}