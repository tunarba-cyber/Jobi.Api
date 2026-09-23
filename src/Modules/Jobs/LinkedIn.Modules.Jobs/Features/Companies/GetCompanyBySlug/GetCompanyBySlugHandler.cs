using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Companies.GetCompanyBySlug;

internal sealed class GetCompanyBySlugHandler : IRequestHandler<GetCompanyBySlugQuery, Result<CompanyDto>>
{
    private readonly JobsDbContext _db;

    public GetCompanyBySlugHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CompanyDto>> Handle(GetCompanyBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        var dto = await _db.Companies
            .AsNoTracking()
            .Where(c => c.Slug == slug)
            .Select(CompanyMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<CompanyDto>(CompanyErrors.NotFoundBySlug(slug))
            : Result.Success(dto);
    }
}
