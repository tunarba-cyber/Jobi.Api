using LinkedIn.Modules.Jobs.Features.Categories.CreateCategory;
using LinkedIn.Modules.Jobs.Features.Categories.DeleteCategory;
using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Modules.Jobs.Features.Categories.GetCategories;
using LinkedIn.Modules.Jobs.Features.Categories.GetCategoryBySlug;
using LinkedIn.Modules.Jobs.Features.Categories.UpdateCategory;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Jobs.Features.Categories;

internal static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/categories")
            .WithTags("Categories")
            ;

        group.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .WithSummary("Lists categories. Use onlyFeatured=true&pageSize=6 for the homepage cards.")
            .Produces<PagedResult<CategoryDto>>();

        group.MapGet("/{slug}", GetCategoryBySlug)
            .WithName("GetCategoryBySlug")
            .Produces<CategoryDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireAdmin");

        group.MapPut("/{id:long}", UpdateCategory)
            .WithName("UpdateCategory")
            .Produces<CategoryDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireAdmin");

        group.MapDelete("/{id:long}", DeleteCategory)
            .WithName("DeleteCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("RequireAdmin");
    }

    private static async Task<IResult> GetCategories(
        ISender sender,
        CancellationToken cancellationToken,
        string? search = null,
        bool onlyFeatured = false,
        bool includeInactive = false,
        string? sortBy = null,
        bool descending = false,
        int page = 1,
        int pageSize = 10)
    {
        var query = new GetCategoriesQuery
        {
            Search = search,
            OnlyFeatured = onlyFeatured,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            Descending = descending,
            Page = page,
            PageSize = pageSize
        };

        var result = await sender.Send(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCategoryBySlug(
        string slug,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryBySlugQuery(slug), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToCreatedResult(dto => $"/api/categories/{dto.Slug}");
    }

    private static async Task<IResult> UpdateCategory(
        long id,
        UpdateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // Route id always wins over body id - prevents a mismatched payload
        // from updating the wrong row.
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteCategory(
        long id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return result.ToHttpResult();
    }
}
