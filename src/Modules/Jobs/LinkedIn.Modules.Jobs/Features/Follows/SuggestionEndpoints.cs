using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Follows;

public sealed record SuggestionDto(string UserId, string Name, string Role, string Reason, int FollowerCount);
public sealed record CompanySuggestionDto(long CompanyId, string Name, string Slug, string? LogoUrl, string Reason, int FollowerCount, int OpenJobs);

internal static class SuggestionEndpoints
{
    public static void MapSuggestionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/follows/suggestions").WithTags("Follows").RequireAuthorization();
        g.MapGet("/users", SuggestUsers);
        g.MapGet("/companies", SuggestCompanies);
    }

    private static string Uid(ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private static async Task<IResult> SuggestUsers(ClaimsPrincipal user, JobsDbContext db, IUserDirectory dir, CancellationToken ct, int take = 6)
    {
        var me = Uid(user);
        take = Math.Clamp(take, 1, 20);

        var following = await db.UserFollows.AsNoTracking().Where(f => f.FollowerUserId == me)
            .Select(f => f.FollowedUserId).ToListAsync(ct);
        var exclude = following.Append(me).ToList();
        var picks = new List<(string Id, string Reason)>();

        if (following.Count > 0)
        {
            var mutual = await db.UserFollows.AsNoTracking()
                .Where(f => following.Contains(f.FollowerUserId) && !exclude.Contains(f.FollowedUserId))
                .GroupBy(f => f.FollowedUserId).Select(g => new { Id = g.Key, N = g.Count() })
                .OrderByDescending(x => x.N).Take(take * 3).ToListAsync(ct);
            picks.AddRange(mutual.Select(m => (m.Id, m.N == 1 ? "Followed by 1 person you follow" : $"Followed by {m.N} people you follow")));
        }

        var taken = picks.Select(p => p.Id).Concat(exclude).ToList();
        var popular = await db.UserFollows.AsNoTracking().Where(f => !taken.Contains(f.FollowedUserId))
            .GroupBy(f => f.FollowedUserId).Select(g => new { Id = g.Key, N = g.Count() })
            .OrderByDescending(x => x.N).Take(take * 3).ToListAsync(ct);
        picks.AddRange(popular.Select(p => (p.Id, "Popular on Jobi")));

        var cards = await dir.GetCardsAsync(picks.Select(p => p.Id).Distinct(), ct); // admins are never returned
        var chosen = new List<(UserCard Card, string Reason)>();
        foreach (var (id, reason) in picks)
            if (chosen.Count < take && cards.TryGetValue(id, out var c) && chosen.All(x => x.Card.Id != id)) chosen.Add((c, reason));

        if (chosen.Count < take)
        {
            var (extra, _) = await dir.BrowseAsync(null, me, 0, 40, ct);
            foreach (var c in extra)
                if (chosen.Count < take && !exclude.Contains(c.Id) && chosen.All(x => x.Card.Id != c.Id)) chosen.Add((c, "New on Jobi"));
        }

        var ids = chosen.Select(x => x.Card.Id).ToList();
        var counts = await db.UserFollows.AsNoTracking().Where(f => ids.Contains(f.FollowedUserId))
            .GroupBy(f => f.FollowedUserId).Select(g => new { Id = g.Key, N = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.N, ct);

        return Results.Ok(chosen.Select(x => new SuggestionDto(x.Card.Id, $"{x.Card.FirstName} {x.Card.LastName}",
            x.Card.Role, x.Reason, counts.GetValueOrDefault(x.Card.Id))).ToList());
    }

    private static async Task<IResult> SuggestCompanies(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct, int take = 6)
    {
        var me = Uid(user);
        take = Math.Clamp(take, 1, 20);

        var following = await db.UserFollows.AsNoTracking().Where(f => f.FollowerUserId == me)
            .Select(f => f.FollowedUserId).ToListAsync(ct);

        var rows = await db.Companies.AsNoTracking()
            .Where(c => c.OwnerUserId != me && !db.CompanyFollows.Any(f => f.CompanyId == c.Id && f.FollowerUserId == me))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.LogoUrl,
                Followers = db.CompanyFollows.Count(f => f.CompanyId == c.Id),
                Mutual = db.CompanyFollows.Count(f => f.CompanyId == c.Id && following.Contains(f.FollowerUserId)),
                Jobs = db.Jobs.Count(j => j.CompanyId == c.Id && j.Status == JobStatus.Active)
            })
            .OrderByDescending(x => x.Mutual).ThenByDescending(x => x.Followers).ThenByDescending(x => x.Jobs)
            .Take(take).ToListAsync(ct);

        return Results.Ok(rows.Select(x => new CompanySuggestionDto(x.Id, x.Name, x.Slug, x.LogoUrl,
            x.Mutual > 0 ? (x.Mutual == 1 ? "Followed by 1 person you follow" : $"Followed by {x.Mutual} people you follow")
            : x.Jobs > 0 ? $"{x.Jobs} open position{(x.Jobs == 1 ? "" : "s")}"
            : x.Followers > 0 ? "Popular company" : "New on Jobi",
            x.Followers, x.Jobs)).ToList());
    }
}