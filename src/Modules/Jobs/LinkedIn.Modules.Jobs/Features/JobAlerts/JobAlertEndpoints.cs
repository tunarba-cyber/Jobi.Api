using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.JobAlerts.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.JobAlerts;

internal static class JobAlertEndpoints
{
    public static void MapJobAlertEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/job-alerts").WithTags("JobAlerts").RequireAuthorization("RequireCandidate");

        group.MapGet("/", GetMine).WithName("GetMyJobAlerts").Produces<List<JobAlertDto>>();
        group.MapPost("/", Create).WithName("CreateJobAlert").Produces<JobAlertDto>(StatusCodes.Status201Created);
        group.MapPut("/{id:long}", Update).WithName("UpdateJobAlert").Produces<JobAlertDto>().ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:long}", Delete).WithName("DeleteJobAlert").Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetMine(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var alerts = await db.JobAlerts
            .AsNoTracking()
            .Where(a => a.CandidateUserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new JobAlertDto(a.Id, a.Keyword, a.CategoryId, a.Location, a.IsActive, a.CreatedAtUtc))
            .ToListAsync(ct);

        return Results.Ok(alerts);
    }

    private static async Task<IResult> Create(UpsertJobAlertRequest request, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var alert = new JobAlert
        {
            CandidateUserId = userId,
            Keyword = request.Keyword?.Trim(),
            CategoryId = request.CategoryId,
            Location = request.Location?.Trim(),
            IsActive = request.IsActive
        };

        db.JobAlerts.Add(alert);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/job-alerts/{alert.Id}",
            new JobAlertDto(alert.Id, alert.Keyword, alert.CategoryId, alert.Location, alert.IsActive, alert.CreatedAtUtc));
    }

    private static async Task<IResult> Update(long id, UpsertJobAlertRequest request, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var alert = await db.JobAlerts.FirstOrDefaultAsync(a => a.Id == id && a.CandidateUserId == userId, ct);
        if (alert is null)
            return Results.Problem(title: "JobAlert.NotFound", statusCode: StatusCodes.Status404NotFound);

        alert.Keyword = request.Keyword?.Trim();
        alert.CategoryId = request.CategoryId;
        alert.Location = request.Location?.Trim();
        alert.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return Results.Ok(new JobAlertDto(alert.Id, alert.Keyword, alert.CategoryId, alert.Location, alert.IsActive, alert.CreatedAtUtc));
    }

    private static async Task<IResult> Delete(long id, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var alert = await db.JobAlerts.FirstOrDefaultAsync(a => a.Id == id && a.CandidateUserId == userId, ct);
        if (alert is null)
            return Results.NoContent(); // deleting something already gone is a no-op

        db.JobAlerts.Remove(alert); // soft-deleted by SoftDeleteInterceptor
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}