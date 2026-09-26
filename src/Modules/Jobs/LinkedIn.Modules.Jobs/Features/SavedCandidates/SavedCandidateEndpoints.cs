using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.SavedCandidates.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.SavedCandidates;

internal static class SavedCandidateEndpoints
{
    public static void MapSavedCandidateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/saved-candidates").WithTags("SavedCandidates").RequireAuthorization("RequireEmployer");

        group.MapGet("/", GetMine).WithName("GetMySavedCandidates").Produces<List<SavedCandidateDto>>();
        group.MapPost("/{candidateProfileId:long}", Save).WithName("SaveCandidate").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{candidateProfileId:long}", Unsave).WithName("UnsaveCandidate").Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetMine(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var items = await db.SavedCandidates
            .AsNoTracking()
            .Where(s => s.EmployerUserId == userId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new SavedCandidateDto(s.Id, s.CandidateProfileId, s.CandidateProfile!.FullName, s.CandidateProfile.Headline, s.CreatedAtUtc))
            .ToListAsync(ct);

        return Results.Ok(items);
    }

    private static async Task<IResult> Save(long candidateProfileId, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var exists = await db.CandidateProfiles.AnyAsync(c => c.Id == candidateProfileId, ct);
        if (!exists)
            return Results.Problem(title: "Candidate.NotFound", statusCode: StatusCodes.Status404NotFound);

        var alreadySaved = await db.SavedCandidates.AnyAsync(s => s.CandidateProfileId == candidateProfileId && s.EmployerUserId == userId, ct);
        if (alreadySaved)
            return SavedItemErrors.AlreadySaved.ToProblem();

        db.SavedCandidates.Add(new SavedCandidate { CandidateProfileId = candidateProfileId, EmployerUserId = userId });
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Unsave(long candidateProfileId, ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var saved = await db.SavedCandidates.FirstOrDefaultAsync(s => s.CandidateProfileId == candidateProfileId && s.EmployerUserId == userId, ct);
        if (saved is null)
            return Results.NoContent();

        db.SavedCandidates.Remove(saved);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}