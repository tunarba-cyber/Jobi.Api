using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Companies;

public sealed record AdminCompanyDto(long Id, string Name, string Slug, string OwnerUserId, string? WebsiteUrl, int JobCount, DateTimeOffset CreatedAtUtc);

internal static class CompanyAdminEndpoints
{
    public static void MapCompanyAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/companies").WithTags("AdminCompanies").RequireAuthorization("RequireAdmin");
        group.MapGet("/", GetAll);
        group.MapDelete("/{id:long}", Delete);
    }

    private static async Task<IResult> GetAll(JobsDbContext db, CancellationToken ct, string? search = null, int page = 1, int pageSize = 15)
    {
        var s = search?.Trim();
        var query = db.Companies.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(s), c => c.Name.Contains(s!))
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new AdminCompanyDto(c.Id, c.Name, c.Slug, c.OwnerUserId, c.WebsiteUrl, c.Jobs.Count, c.CreatedAtUtc));

        return Results.Ok(await query.ToPagedResultAsync(Math.Max(page, 1), Math.Clamp(pageSize, 1, 50), ct));
    }

    private static async Task<IResult> Delete(long id, JobsDbContext db, CancellationToken ct)
    {
        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (company is null) return Results.NoContent();

        await db.Jobs.Where(j => j.CompanyId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Closed), ct);

        db.Companies.Remove(company); // soft-deleted by interceptor
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}