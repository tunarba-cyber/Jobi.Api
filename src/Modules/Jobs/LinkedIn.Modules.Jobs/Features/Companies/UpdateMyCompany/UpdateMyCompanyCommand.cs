using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Companies.UpdateMyCompany;

/// <summary>
/// No CompanyId or OwnerUserId here on purpose - the handler looks up the
/// caller's own company by RequestingUserId, same as GetMyCompanyQuery. There
/// is no way for a request body to target someone else's company.
/// </summary>
public sealed record UpdateMyCompanyCommand(
    string RequestingUserId,
    string Name,
    string? Description,
    string? WebsiteUrl,
    string? LogoUrl) : IRequest<Result<CompanyDto>>;