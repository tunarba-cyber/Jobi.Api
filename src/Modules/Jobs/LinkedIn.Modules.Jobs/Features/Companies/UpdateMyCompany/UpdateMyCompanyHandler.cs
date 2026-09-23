using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Companies.UpdateMyCompany;

internal sealed class UpdateMyCompanyHandler : IRequestHandler<UpdateMyCompanyCommand, Result<CompanyDto>>
{
    private readonly JobsDbContext _db;

    public UpdateMyCompanyHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CompanyDto>> Handle(UpdateMyCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.OwnerUserId == request.RequestingUserId, cancellationToken);

        if (company is null)
            return Result.Failure<CompanyDto>(CompanyErrors.NoCompanyYet);

        // Name intentionally does NOT touch Slug - the slug is the company's
        // stable public URL (/api/companies/{slug}); changing it on every rename
        // would break links already shared/bookmarked to this company page.
        company.Name = request.Name.Trim();
        company.Description = request.Description?.Trim();
        company.WebsiteUrl = request.WebsiteUrl?.Trim();
        company.LogoUrl = request.LogoUrl?.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CompanyDto(
            company.Id, company.Name, company.Slug, company.Description,
            company.WebsiteUrl, company.LogoUrl, company.CreatedAtUtc));
    }
}