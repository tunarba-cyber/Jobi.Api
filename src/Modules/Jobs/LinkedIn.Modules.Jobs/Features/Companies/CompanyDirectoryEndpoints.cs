using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Companies;

public sealed record CompanyListItemDto(long Id, string Name, string Slug, string? Description, string? WebsiteUrl, string? LogoUrl,
    int OpenJobs, int Followers, bool IsFollowing);
public sealed record CompanyProfileDto(long Id, string Name, string Slug, string? Description, string? WebsiteUrl, string? LogoUrl,
    int OpenJobs, int Followers, bool IsFollowing, bool IsOwner, DateTimeOffset CreatedAtUtc);

internal static class CompanyDirectoryEndpoints
{
    public static void MapCompanyDirectoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/companies").WithTags("Companies");
        g.MapGet("/", List).AllowAnonymous();
        g.MapGet("/{slug}/profile", Profile).AllowAnonymous();
    }

    // Public, but if the caller sent a token we also say whether they follow each company.
    private static async Task<IResult> List(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct, string? q = null, int page = 1, int pageSize = 12)
    {
        var me = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var s = q?.Trim();

        var query = db.Companies.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(s), c => c.Name.Contains(s!))
            .OrderBy(c => c.Name)
            .Select(c => new CompanyListItemDto(c.Id, c.Name, c.Slug, c.Description, c.WebsiteUrl, c.LogoUrl,
                db.Jobs.Count(j => j.CompanyId == c.Id && j.Status == JobStatus.Active),
                db.CompanyFollows.Count(f => f.CompanyId == c.Id),
                db.CompanyFollows.Any(f => f.CompanyId == c.Id && f.FollowerUserId == me)));

        return Results.Ok(await query.ToPagedResultAsync(Math.Max(page, 1), Math.Clamp(pageSize, 1, 48), ct));
    }

    private static async Task<IResult> Profile(string slug, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var me = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var dto = await db.Companies.AsNoTracking().Where(c => c.Slug == slug)
            .Select(c => new CompanyProfileDto(c.Id, c.Name, c.Slug, c.Description, c.WebsiteUrl, c.LogoUrl,
                db.Jobs.Count(j => j.CompanyId == c.Id && j.Status == JobStatus.Active),
                db.CompanyFollows.Count(f => f.CompanyId == c.Id),
                db.CompanyFollows.Any(f => f.CompanyId == c.Id && f.FollowerUserId == me),
                c.OwnerUserId == me, c.CreatedAtUtc))
            .FirstOrDefaultAsync(ct);

        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }
}