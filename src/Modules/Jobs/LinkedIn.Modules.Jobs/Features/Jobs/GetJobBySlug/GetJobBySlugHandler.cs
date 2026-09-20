using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Jobs.GetJobBySlug;

internal sealed class GetJobBySlugHandler : IRequestHandler<GetJobBySlugQuery, Result<JobDto>>
{
    private readonly JobsDbContext _db;

    public GetJobBySlugHandler(JobsDbContext db) => _db = db;

    public async Task<Result<JobDto>> Handle(GetJobBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        var dto = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.Slug == slug)
            .Select(JobMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<JobDto>(JobErrors.NotFoundBySlug(slug))
            : Result.Success(dto);
    }
}
