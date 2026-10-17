using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LinkedIn.Modules.Jobs.Features.Analytics;

public sealed record JobStatsDto(long JobId, string Title, string Slug, JobStatus Status, int Views, int Applications, int Shortlisted, int Hired);
public sealed record DailyCountDto(DateOnly Day, int Count);
public sealed record EmployerAnalyticsDto(int TotalViews, int TotalApplications, int ActiveJobs, int Shortlisted, int Hired,
    double ConversionPercent, IReadOnlyList<JobStatsDto> Jobs, IReadOnlyList<DailyCountDto> ApplicationsLast30Days);

internal static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/api/analytics/employer", ForEmployer)
            .WithTags("Analytics").RequireAuthorization(AuthPolicies.Employer);

    private static async Task<IResult> ForEmployer(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var jobs = await db.Jobs.AsNoTracking().Where(j => j.Company!.OwnerUserId == uid)
            .Select(j => new { j.Id, j.Title, j.Slug, j.Status, j.ViewCount }).ToListAsync(ct);

        var ids = jobs.Select(j => j.Id).ToList();
        var apps = await db.Applications.AsNoTracking().Where(a => ids.Contains(a.JobId))
            .Select(a => new { a.JobId, a.Status, a.CreatedAtUtc }).ToListAsync(ct);
        var byJob = apps.ToLookup(a => a.JobId);

        var rows = jobs.Select(j => new JobStatsDto(j.Id, j.Title, j.Slug, j.Status, j.ViewCount,
                byJob[j.Id].Count(),
                byJob[j.Id].Count(a => a.Status == ApplicationStatus.Shortlisted),
                byJob[j.Id].Count(a => a.Status == ApplicationStatus.Hired)))
            .OrderByDescending(r => r.Applications).ThenByDescending(r => r.Views).ToList();

        var offset = TimeSpan.FromHours(4); // Baku
        var today = DateOnly.FromDateTime(DateTime.UtcNow + offset);
        var perDay = apps.GroupBy(a => DateOnly.FromDateTime((a.CreatedAtUtc + offset).DateTime))
            .ToDictionary(g => g.Key, g => g.Count());
        var series = Enumerable.Range(0, 30).Select(i => today.AddDays(i - 29))
            .Select(d => new DailyCountDto(d, perDay.GetValueOrDefault(d))).ToList();

        var totalViews = rows.Sum(r => r.Views);
        var totalApps = rows.Sum(r => r.Applications);

        return Results.Ok(new EmployerAnalyticsDto(
            totalViews, totalApps, rows.Count(r => r.Status == JobStatus.Active),
            rows.Sum(r => r.Shortlisted), rows.Sum(r => r.Hired),
            totalViews == 0 ? 0 : Math.Round(totalApps * 100.0 / totalViews, 1), rows, series));
    }
}