using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Follows;

public sealed record FollowSummaryDto(int Followers, int Following, int FollowedCompanies, int CompanyFollowers);
public sealed record FollowUserDto(string UserId, string Name, string Role, bool IsFollowing, int FollowerCount);
public sealed record FollowCompanyDto(long CompanyId, string Name, string Slug, bool IsFollowing, int FollowerCount);

internal static class FollowEndpoints
{
    public static void MapFollowEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/follows").WithTags("Follows").RequireAuthorization();
        g.MapGet("/summary", Summary);
        g.MapGet("/users", ExploreUsers);
        g.MapPost("/users/{id}", FollowUser);
        g.MapDelete("/users/{id}", UnfollowUser);
        g.MapGet("/followers", Followers);
        g.MapGet("/following", Following);
        g.MapGet("/companies", ExploreCompanies);
        g.MapPost("/companies/{id:long}", FollowCompany);
        g.MapDelete("/companies/{id:long}", UnfollowCompany);
        g.MapGet("/followed-companies", FollowedCompanies);
    }

    private static string Uid(ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private static IResult Bad(string detail) => Results.Problem(title: "Follow.Invalid", detail: detail, statusCode: 400);

    private static async Task<IResult> Summary(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        return Results.Ok(new FollowSummaryDto(
            await db.UserFollows.CountAsync(f => f.FollowedUserId == me, ct),
            await db.UserFollows.CountAsync(f => f.FollowerUserId == me, ct),
            await db.CompanyFollows.CountAsync(f => f.FollowerUserId == me, ct),
            await db.CompanyFollows.CountAsync(f => f.Company!.OwnerUserId == me, ct)));
    }

    // ---- users ----
    private static async Task<List<FollowUserDto>> ToDtos(IReadOnlyCollection<UserCard> cards, string me, JobsDbContext db, CancellationToken ct)
    {
        var ids = cards.Select(c => c.Id).ToList();
        var counts = await db.UserFollows.AsNoTracking().Where(f => ids.Contains(f.FollowedUserId))
            .GroupBy(f => f.FollowedUserId).Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, ct);
        var mine = (await db.UserFollows.AsNoTracking().Where(f => f.FollowerUserId == me && ids.Contains(f.FollowedUserId))
            .Select(f => f.FollowedUserId).ToListAsync(ct)).ToHashSet();

        return cards.Select(c => new FollowUserDto(c.Id, $"{c.FirstName} {c.LastName}", c.Role,
            mine.Contains(c.Id), counts.GetValueOrDefault(c.Id))).ToList();
    }

    private static async Task<IResult> ExploreUsers(ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct,
        string? q = null, int page = 1, int pageSize = 12)
    {
        var me = Uid(user);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (cards, total) = await dir.BrowseAsync(q, me, (page - 1) * pageSize, pageSize, ct);
        return Results.Ok(new PagedResult<FollowUserDto>(await ToDtos(cards.ToList(), me, db, ct), page, pageSize, total));
    }

    private static async Task<IResult> ListUsers(IQueryable<string> idQuery, string me, JobsDbContext db, IUserDirectory dir, CancellationToken ct)
    {
        var ids = await idQuery.Take(200).ToListAsync(ct);
        var cards = await dir.GetCardsAsync(ids, ct);
        var ordered = ids.Where(cards.ContainsKey).Select(id => cards[id]).ToList();
        return Results.Ok(await ToDtos(ordered, me, db, ct));
    }

    private static Task<IResult> Followers(ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct)
    {
        var me = Uid(user);
        return ListUsers(db.UserFollows.AsNoTracking().Where(f => f.FollowedUserId == me)
            .OrderByDescending(f => f.CreatedAtUtc).Select(f => f.FollowerUserId), me, db, dir, ct);
    }

    private static Task<IResult> Following(ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct)
    {
        var me = Uid(user);
        return ListUsers(db.UserFollows.AsNoTracking().Where(f => f.FollowerUserId == me)
            .OrderByDescending(f => f.CreatedAtUtc).Select(f => f.FollowedUserId), me, db, dir, ct);
    }

    private static async Task<IResult> FollowUser(string id, ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir,
        INotificationPublisher notify, CancellationToken ct)
    {
        var me = Uid(user);
        if (id == me) return Bad("You can't follow yourself.");
        if (!await dir.IsFollowableAsync(id, ct)) return Bad("This account can't be followed.");
        if (await db.UserFollows.AnyAsync(f => f.FollowerUserId == me && f.FollowedUserId == id, ct)) return Results.NoContent();

        db.UserFollows.Add(new UserFollow { FollowerUserId = me, FollowedUserId = id });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Results.NoContent(); }   // double-click race: already followed

        var who = (await dir.GetSummariesAsync(new[] { me }, ct)).GetValueOrDefault(me);
        await notify.PublishAsync(id, NotificationTypes.Follow, "New follower",
            $"{who?.FirstName} {who?.LastName} started following you.", "/Follow", ct: ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UnfollowUser(string id, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        var f = await db.UserFollows.FirstOrDefaultAsync(x => x.FollowerUserId == me && x.FollowedUserId == id, ct);
        if (f is not null) { db.UserFollows.Remove(f); await db.SaveChangesAsync(ct); }
        return Results.NoContent();
    }

    // ---- companies ----
    private static async Task<IResult> ExploreCompanies(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct,
        string? q = null, int page = 1, int pageSize = 12)
    {
        var me = Uid(user);
        var s = q?.Trim();
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var source = db.Companies.AsNoTracking().Where(c => c.OwnerUserId != me)
            .WhereIf(!string.IsNullOrWhiteSpace(s), c => c.Name.Contains(s!));

        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new FollowCompanyDto(c.Id, c.Name, c.Slug,
                db.CompanyFollows.Any(f => f.CompanyId == c.Id && f.FollowerUserId == me),
                db.CompanyFollows.Count(f => f.CompanyId == c.Id)))
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<FollowCompanyDto>(items, page, pageSize, total));
    }

    private static async Task<IResult> FollowedCompanies(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        return Results.Ok(await db.CompanyFollows.AsNoTracking()
            .Where(f => f.FollowerUserId == me)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new FollowCompanyDto(f.CompanyId, f.Company!.Name, f.Company.Slug, true,
                db.CompanyFollows.Count(x => x.CompanyId == f.CompanyId)))
            .ToListAsync(ct));
    }

    private static async Task<IResult> FollowCompany(long id, ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir,
        INotificationPublisher notify, CancellationToken ct)
    {
        var me = Uid(user);
        var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (company is null) return Results.NotFound();
        if (company.OwnerUserId == me) return Bad("You can't follow your own company.");
        if (await db.CompanyFollows.AnyAsync(f => f.FollowerUserId == me && f.CompanyId == id, ct)) return Results.NoContent();

        db.CompanyFollows.Add(new CompanyFollow { FollowerUserId = me, CompanyId = id });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Results.NoContent(); }

        var who = (await dir.GetSummariesAsync(new[] { me }, ct)).GetValueOrDefault(me);
        await notify.PublishAsync(company.OwnerUserId, NotificationTypes.Follow, "New company follower",
            $"{who?.FirstName} {who?.LastName} started following {company.Name}.", "/Follow", ct: ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UnfollowCompany(long id, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = Uid(user);
        var f = await db.CompanyFollows.FirstOrDefaultAsync(x => x.FollowerUserId == me && x.CompanyId == id, ct);
        if (f is not null) { db.CompanyFollows.Remove(f); await db.SaveChangesAsync(ct); }
        return Results.NoContent();
    }
}