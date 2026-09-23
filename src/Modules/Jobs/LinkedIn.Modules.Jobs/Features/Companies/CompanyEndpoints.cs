using LinkedIn.Modules.Jobs.Features.Companies.CreateCompany;
using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Modules.Jobs.Features.Companies.GetCompanyBySlug;
using LinkedIn.Modules.Jobs.Features.Companies.GetMyCompany;
using LinkedIn.Modules.Jobs.Features.Companies.UpdateMyCompany;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace LinkedIn.Modules.Jobs.Features.Companies;

internal static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/companies")
            .WithTags("Companies")
            ;

        group.MapGet("/{slug}", GetBySlug)
            .WithName("GetCompanyBySlug")
            .Produces<CompanyDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", GetMine)
            .WithName("GetMyCompany")
            .WithSummary("The calling employer's own company profile, if they have created one.")
            .Produces<CompanyDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("RequireEmployer");

        group.MapPost("/", Create)
            .WithName("CreateCompany")
            .Produces<CompanyDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("RequireEmployer");
        group.MapPut("/me", UpdateMine)
            .WithName("UpdateMyCompany")
            .Produces<CompanyDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("RequireEmployer");
    }

    private static async Task<IResult> GetBySlug(string slug, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetCompanyBySlugQuery(slug), ct)).ToHttpResult();

    private static async Task<IResult> GetMine(ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await sender.Send(new GetMyCompanyQuery(userId), ct)).ToHttpResult();
    }

    private static async Task<IResult> Create(
        CreateCompanyCommand command,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(command with { OwnerUserId = userId }, ct);
        return result.ToCreatedResult(dto => $"/api/companies/{dto.Slug}");
    }
    private static async Task<IResult> UpdateMine(
        UpdateMyCompanyCommand command,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(command with { RequestingUserId = userId }, ct);
        return result.ToHttpResult();
    }
}
