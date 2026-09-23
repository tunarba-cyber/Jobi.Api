using System.Linq.Expressions;
using LinkedIn.Modules.Jobs.Domain.Entities;

namespace LinkedIn.Modules.Jobs.Features.Companies.Dtos;

internal static class CompanyMappings
{
    // OwnerUserId is deliberately excluded from the public DTO - no reason to
    // expose another user's account id through a public company profile.
    public static readonly Expression<Func<Company, CompanyDto>> ToDto =
        company => new CompanyDto(
            company.Id,
            company.Name,
            company.Slug,
            company.Description,
            company.WebsiteUrl,
            company.LogoUrl,
            company.CreatedAtUtc);

    public static CompanyDto ToCompanyDto(this Company company) =>
        new(company.Id,
            company.Name,
            company.Slug,
            company.Description,
            company.WebsiteUrl,
            company.LogoUrl,
            company.CreatedAtUtc);
}
