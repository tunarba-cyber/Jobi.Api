using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Applications.ApplyToJob;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Modules.Jobs.Features.Applications.GetApplicantsForJob;
using LinkedIn.Modules.Jobs.Features.Applications.GetMyApplications;
using LinkedIn.Modules.Jobs.Features.Applications.UpdateApplicationStatus;
using LinkedIn.Modules.Jobs.Features.Applications.WithdrawApplication;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Jobs.Features.Applications;

internal static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/applications")
            .WithTags("Applications");

        group.MapPost("/", Apply)
            .WithName("ApplyToJob")
            .Produces<ApplicationDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("RequireCandidate");

        group.MapDelete("/{id:long}", Withdraw)
            .WithName("WithdrawApplication")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireCandidate");

        group.MapGet("/mine", GetMine)
            .WithName("GetMyApplications")
            .Produces<PagedResult<ApplicationDto>>()
            .RequireAuthorization("RequireCandidate");

        group.MapGet("/job/{jobId:long}", GetApplicantsForJob)
            .WithName("GetApplicantsForJob")
            .Produces<PagedResult<ApplicationDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireEmployer");

        group.MapPut("/{id:long}/status", UpdateStatus)
            .WithName("UpdateApplicationStatus")
            .Produces<ApplicationDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireEmployer");
    }

    private static async Task<IResult> Apply(
        ApplyToJobCommand command,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var name = user.Identity?.Name ?? email;

        var result = await sender.Send(
            command with { CandidateUserId = userId, CandidateName = name, CandidateEmail = email }, ct);
        return result.ToCreatedResult(dto => $"/api/applications/{dto.Id}");
    }

    private static async Task<IResult> Withdraw(long id, ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new WithdrawApplicationCommand(id, userId), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMine(
        ClaimsPrincipal user, ISender sender, CancellationToken ct,
        ApplicationStatus? status = null, int page = 1, int pageSize = 10)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetMyApplicationsQuery { RequestingUserId = userId, Status = status, Page = page, PageSize = pageSize };
        return (await sender.Send(query, ct)).ToHttpResult();
    }

    private static async Task<IResult> GetApplicantsForJob(
        long jobId, ClaimsPrincipal user, ISender sender, CancellationToken ct,
        ApplicationStatus? status = null, int page = 1, int pageSize = 10)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetApplicantsForJobQuery(jobId) { RequestingUserId = userId, Status = status, Page = page, PageSize = pageSize };
        return (await sender.Send(query, ct)).ToHttpResult();
    }

    private static async Task<IResult> UpdateStatus(
        long id, UpdateApplicationStatusCommand command, ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(command with { ApplicationId = id, RequestingUserId = userId }, ct);
        return result.ToHttpResult();
    }
}