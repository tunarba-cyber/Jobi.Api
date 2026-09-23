using LinkedIn.Modules.Jobs.Features.Companies.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Companies.GetMyCompany;

/// <summary>RequestingUserId is set by the endpoint from the caller's JWT.</summary>
public sealed record GetMyCompanyQuery(string RequestingUserId) : IRequest<Result<CompanyDto>>;
