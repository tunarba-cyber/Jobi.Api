using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Companies.GetCompanyBySlug;

/// <summary>Public lookup - backs company-details.html.</summary>
public sealed record GetCompanyBySlugQuery(string Slug) : IRequest<Result<CompanyDto>>;
