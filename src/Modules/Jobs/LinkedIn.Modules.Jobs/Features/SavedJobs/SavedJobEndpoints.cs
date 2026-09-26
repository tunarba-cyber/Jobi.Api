using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.SavedJobs.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.SavedJobs;

internal static class SavedJobEndpoints
{
    public static void MapSavedJobEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/saved-jobs").WithTags("SavedJobs").RequireAuthorization("RequireCandidate");

        group.MapGet("/", GetMine).WithName("GetMySavedJobs").Produces<List<SavedJobDto>>();
        group.MapPost("/{jobId:long}", Save).WithName("SaveJob").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{jobId:long}", Unsave).WithName("UnsaveJob").Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetMine(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var items = await db.SavedJobs
            .AsNoTracking()
            .Where(s => s.CandidateUserId == userId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new SavedJobDto(s.Id, s.JobId, s.Job!.Title, s.Job.Company!.Name, s.CreatedAtUtc))
            .ToListAsync(ct);

        return Results.Ok(items);
    }

    private static async Task<IResult> Save(long jobId, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var jobExists = await db.Jobs.AnyAsync(j => j.Id == jobId, ct);
        if (!jobExists)
            return Results.Problem(title: "Job.NotFound", statusCode: StatusCodes.Status404NotFound);

        var alreadySaved = await db.SavedJobs.AnyAsync(s => s.JobId == jobId && s.CandidateUserId == userId, ct);
        if (alreadySaved)
            return SavedItemErrors.AlreadySaved.ToProblem();

        db.SavedJobs.Add(new SavedJob { JobId = jobId, CandidateUserId = userId });
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Unsave(long jobId, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var saved = await db.SavedJobs.FirstOrDefaultAsync(s => s.JobId == jobId && s.CandidateUserId == userId, ct);
        if (saved is null)
            return Results.NoContent(); // unsaving something not saved is a no-op, not an error

        db.SavedJobs.Remove(saved);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}