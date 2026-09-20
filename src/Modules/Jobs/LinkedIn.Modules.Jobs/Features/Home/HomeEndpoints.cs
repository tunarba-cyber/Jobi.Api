using LinkedIn.Modules.Jobs.Features.Home.Dtos;
using LinkedIn.Modules.Jobs.Features.Home.GetHomeSummary;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Jobs.Features.Home;

internal static class HomeEndpoints
{
    public static void MapHomeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/home")
            .WithTags("Home")
            ;

        group.MapGet("/summary", GetHomeSummary)
            .WithName("GetHomeSummary")
            .WithSummary("Everything index.html needs in one call: counters, featured categories, featured jobs.")
            .Produces<HomeSummaryDto>();
    }

    private static async Task<IResult> GetHomeSummary(
        ISender sender,
        CancellationToken cancellationToken,
        int featuredCategoriesCount = 6,
        int featuredJobsCount = 6)
    {
        var result = await sender.Send(
            new GetHomeSummaryQuery(featuredCategoriesCount, featuredJobsCount),
            cancellationToken);

        return result.ToHttpResult();
    }
}
