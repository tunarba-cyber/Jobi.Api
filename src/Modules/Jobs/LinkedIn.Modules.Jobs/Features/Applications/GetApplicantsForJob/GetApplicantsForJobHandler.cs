using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using LinkedIn.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Applications.GetApplicantsForJob;

internal sealed class GetApplicantsForJobHandler : IRequestHandler<GetApplicantsForJobQuery, Result<PagedResult<ApplicationDto>>>
{
    private readonly JobsDbContext _db;

    public GetApplicantsForJobHandler(JobsDbContext db) => _db = db;

    public async Task<Result<PagedResult<ApplicationDto>>> Handle(GetApplicantsForJobQuery request, CancellationToken cancellationToken)
    {
        // Direct join, no cross-module contract - Job/Company/Application all
        // live in the same JobsDbContext, so this is just a plain EF query.
        var job = await _db.Jobs.AsNoTracking()
            .Where(j => j.Id == request.JobId)
            .Select(j => new { j.Id, j.Company!.OwnerUserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return Result.Failure<PagedResult<ApplicationDto>>(JobErrors.NotFound(request.JobId));

        if (job.OwnerUserId != request.RequestingUserId)
            return Result.Failure<PagedResult<ApplicationDto>>(ApplicationErrors.JobNotOwnedByCaller);

        var query = _db.Applications
            .AsNoTracking()
            .Where(a => a.JobId == request.JobId)
            .WhereIf(request.Status is not null, a => a.Status == request.Status)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(ApplicationMappings.ToDto);

        var page = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);
        return Result.Success(page);
    }
}