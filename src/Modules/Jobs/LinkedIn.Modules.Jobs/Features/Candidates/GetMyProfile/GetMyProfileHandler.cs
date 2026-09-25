using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Candidates.GetMyProfile;

internal sealed class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, Result<CandidateProfileDto>>
{
    private readonly JobsDbContext _db;

    public GetMyProfileHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CandidateProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var dto = await _db.CandidateProfiles
            .AsNoTracking()
            .Where(c => c.OwnerUserId == request.RequestingUserId)
            .Select(CandidateMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<CandidateProfileDto>(CandidateErrors.NoProfileYet)
            : Result.Success(dto);
    }
}