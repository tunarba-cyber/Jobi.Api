using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Companies.CreateCompany;

/// <summary>OwnerUserId is set by the endpoint from the caller's JWT, never the request body.</summary>
public sealed record CreateCompanyCommand(
    string OwnerUserId,
    string Name,
    string? Slug,
    string? Description,
    string? WebsiteUrl,
    string? LogoUrl) : IRequest<Result<CompanyDto>>;
