using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using LinkedIn.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Applications.GetMyApplications;

internal sealed class GetMyApplicationsHandler : IRequestHandler<GetMyApplicationsQuery, Result<PagedResult<ApplicationDto>>>
{
    private readonly JobsDbContext _db;

    public GetMyApplicationsHandler(JobsDbContext db) => _db = db;

    public async Task<Result<PagedResult<ApplicationDto>>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Applications
            .AsNoTracking()
            .Where(a => a.CandidateUserId == request.RequestingUserId)
            .WhereIf(request.Status is not null, a => a.Status == request.Status)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(ApplicationMappings.ToDto);

        var page = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);
        return Result.Success(page);
    }
}