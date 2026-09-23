using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Companies.GetMyCompany;

internal sealed class GetMyCompanyHandler : IRequestHandler<GetMyCompanyQuery, Result<CompanyDto>>
{
    private readonly JobsDbContext _db;

    public GetMyCompanyHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CompanyDto>> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
    {
        var dto = await _db.Companies
            .AsNoTracking()
            .Where(c => c.OwnerUserId == request.RequestingUserId)
            .Select(CompanyMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<CompanyDto>(CompanyErrors.NoCompanyYet)
            : Result.Success(dto);
    }
}
