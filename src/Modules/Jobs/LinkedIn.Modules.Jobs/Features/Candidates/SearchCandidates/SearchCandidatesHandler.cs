using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using LinkedIn.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Candidates.SearchCandidates;

internal sealed class SearchCandidatesHandler : IRequestHandler<SearchCandidatesQuery, Result<PagedResult<CandidateProfileDto>>>
{
    private readonly JobsDbContext _db;

    public SearchCandidatesHandler(JobsDbContext db) => _db = db;

    public async Task<Result<PagedResult<CandidateProfileDto>>> Handle(SearchCandidatesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim();

        var query = _db.CandidateProfiles
            .AsNoTracking()
            .Where(c => c.IsAvailableForWork)
            .WhereIf(!string.IsNullOrWhiteSpace(search), c => c.FullName.Contains(search!) || c.Headline.Contains(search!))
            .WhereIf(!string.IsNullOrWhiteSpace(request.Location), c => c.Location != null && c.Location.Contains(request.Location!))
            .WhereIf(request.ExperienceLevel is not null, c => c.ExperienceLevel == request.ExperienceLevel)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(CandidateMappings.ToDto);

        var page = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);
        return Result.Success(page);
    }
}