using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Categories;
using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Companies.CreateCompany;

internal sealed class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
{
    private readonly JobsDbContext _db;

    public CreateCompanyHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        // One company per employer - enforced here and again at the database
        // level via the unique index on OwnerUserId (see CompanyConfiguration).
        var alreadyHasCompany = await _db.Companies
            .AnyAsync(c => c.OwnerUserId == request.OwnerUserId, cancellationToken);

        if (alreadyHasCompany)
            return Result.Failure<CompanyDto>(CompanyErrors.AlreadyHasCompany);

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        if (await _db.Companies.AnyAsync(c => c.Slug == slug, cancellationToken))
            return Result.Failure<CompanyDto>(CompanyErrors.SlugAlreadyExists(slug));

        var company = new Company
        {
            OwnerUserId = request.OwnerUserId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            WebsiteUrl = request.WebsiteUrl?.Trim(),
            LogoUrl = request.LogoUrl?.Trim()
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(company.ToCompanyDto());
    }
}
