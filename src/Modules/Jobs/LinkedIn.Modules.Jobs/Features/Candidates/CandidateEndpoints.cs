using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Modules.Jobs.Features.Candidates.GetCandidateBySlug;
using LinkedIn.Modules.Jobs.Features.Candidates.GetMyProfile;
using LinkedIn.Modules.Jobs.Features.Candidates.SearchCandidates;
using LinkedIn.Modules.Jobs.Features.Candidates.UpsertMyProfile;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Jobs.Features.Candidates;

internal static class CandidateEndpoints
{
    public static void MapCandidateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/candidates").WithTags("Candidates");

        group.MapGet("/", Search)
            .WithName("SearchCandidates")
            .Produces<PagedResult<CandidateProfileDto>>();

        group.MapGet("/{slug}", GetBySlug)
            .WithName("GetCandidateBySlug")
            .Produces<CandidateProfileDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", GetMine)
            .WithName("GetMyCandidateProfile")
            .Produces<CandidateProfileDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("RequireCandidate");

        group.MapPut("/me", UpsertMine)
            .WithName("UpsertMyCandidateProfile")
            .Produces<CandidateProfileDto>()
            .ProducesValidationProblem()
            .RequireAuthorization("RequireCandidate");
    }

    private static async Task<IResult> Search(
        ISender sender, CancellationToken ct,
        string? search = null, string? location = null, ExperienceLevel? experienceLevel = null,
        int page = 1, int pageSize = 10) =>
        (await sender.Send(new SearchCandidatesQuery { Search = search, Location = location, ExperienceLevel = experienceLevel, Page = page, PageSize = pageSize }, ct)).ToHttpResult();

    private static async Task<IResult> GetBySlug(string slug, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetCandidateBySlugQuery(slug), ct)).ToHttpResult();

    private static async Task<IResult> GetMine(ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await sender.Send(new GetMyProfileQuery(userId), ct)).ToHttpResult();
    }

    private static async Task<IResult> UpsertMine(UpsertMyProfileCommand command, ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(command with { RequestingUserId = userId }, ct);
        return result.ToHttpResult();
    }
}