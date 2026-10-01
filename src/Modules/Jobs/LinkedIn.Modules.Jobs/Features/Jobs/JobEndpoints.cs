using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Jobs.CreateJob;
using LinkedIn.Modules.Jobs.Features.Jobs.DeleteJob;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Modules.Jobs.Features.Jobs.GetJobBySlug;
using LinkedIn.Modules.Jobs.Features.Jobs.GetJobs;
using LinkedIn.Modules.Jobs.Features.Jobs.UpdateJob;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Jobs;

internal static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/jobs")
            .WithTags("Jobs")
            ;

        group.MapGet("/", GetJobs)
            .WithName("GetJobs")
            .WithSummary("Lists jobs. Use onlyFeatured=true&pageSize=6 for the homepage carousel.")
            .Produces<PagedResult<JobDto>>();

        group.MapGet("/{slug}", GetJobBySlug)
            .WithName("GetJobBySlug")
            .Produces<JobDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateJob)
            .WithName("CreateJob")
            .Produces<JobDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireEmployer");

        group.MapPut("/{id:long}", UpdateJob)
            .WithName("UpdateJob")
            .Produces<JobDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireEmployer");

        group.MapDelete("/{id:long}", DeleteJob)
            .WithName("DeleteJob")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireEmployer");
        group.MapGet("/mine", GetMyJobs)
    .WithName("GetMyJobs")
    .WithSummary("The calling employer's own jobs in every status (Draft, Active, Closed).")
    .Produces<PagedResult<JobDto>>()
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .RequireAuthorization("RequireEmployer");

        group.MapGet("/mine/{id:long}", GetMyJob)
            .WithName("GetMyJob")
            .Produces<JobDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("RequireEmployer");
    }

    private static async Task<IResult> GetJobs(
        ISender sender,
        
        CancellationToken cancellationToken,
        string? companySlug = null,
        string? search = null,
        string? categorySlug = null,
        JobType? jobType = null,
        ExperienceLevel? experienceLevel = null,
        string? location = null,
        bool onlyFeatured = false,
        bool includeAllStatuses = false,
        
        string? sortBy = null,
        bool descending = false,
        int page = 1,
        int pageSize = 10)
    {
        var query = new GetJobsQuery
        {
            Search = search,
            CategorySlug = categorySlug,
            JobType = jobType,
            ExperienceLevel = experienceLevel,
            Location = location,
            OnlyFeatured = onlyFeatured,
            IncludeAllStatuses = includeAllStatuses,
            SortBy = sortBy,
            Descending = descending,
            Page = page,
            PageSize = pageSize,
            CompanySlug = companySlug,
        };

        var result = await sender.Send(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetJobBySlug(
        string slug,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetJobBySlugQuery(slug), cancellationToken);
        return result.ToHttpResult();
    }
    private static async Task<IResult> GetMyJobs(
    ClaimsPrincipal user,
    JobsDbContext db,
    CancellationToken cancellationToken,
    int page = 1,
    int pageSize = 10)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Jobs
            .AsNoTracking()
            .Where(j => j.Company!.OwnerUserId == userId)
            .OrderByDescending(j => j.CreatedAtUtc)
            .Select(JobMappings.ToDto);

        return Results.Ok(await query.ToPagedResultAsync(page, pageSize, cancellationToken));
    }

    private static async Task<IResult> GetMyJob(
        long id,
        ClaimsPrincipal user,
        JobsDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var dto = await db.Jobs
            .AsNoTracking()
            .Where(j => j.Id == id && j.Company!.OwnerUserId == userId)
            .Select(JobMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Results.Problem(title: "Job.NotFound", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(dto);
    }

    private static async Task<IResult> CreateJob(
        CreateJobCommand command,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!; // guaranteed present - endpoint requires auth
        var result = await sender.Send(command with { RequestingUserId = userId }, cancellationToken);
        return result.ToCreatedResult(dto => $"/api/jobs/{dto.Slug}");
    }

    private static async Task<IResult> UpdateJob(
        long id,
        UpdateJobCommand command,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        // Route id always wins over body id; RequestingUserId always comes from
        // the token, never the body - both are server-controlled, not client input.
        var result = await sender.Send(command with { Id = id, RequestingUserId = userId }, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteJob(
        long id,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new DeleteJobCommand(id, userId), cancellationToken);
        return result.ToHttpResult();
    }
}
