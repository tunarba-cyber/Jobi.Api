using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Candidates.GetCandidateBySlug;

internal sealed class GetCandidateBySlugHandler : IRequestHandler<GetCandidateBySlugQuery, Result<CandidateProfileDto>>
{
    private readonly JobsDbContext _db;

    public GetCandidateBySlugHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CandidateProfileDto>> Handle(GetCandidateBySlugQuery request, CancellationToken cancellationToken)
    {
        var dto = await _db.CandidateProfiles
            .AsNoTracking()
            .Where(c => c.Slug == request.Slug && c.IsAvailableForWork)
            .Select(CandidateMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<CandidateProfileDto>(CandidateErrors.NotFoundBySlug(request.Slug))
            : Result.Success(dto);
    }
}