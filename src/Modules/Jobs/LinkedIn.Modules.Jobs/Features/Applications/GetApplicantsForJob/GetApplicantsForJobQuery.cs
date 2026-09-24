using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Applications.GetApplicantsForJob;

public sealed record GetApplicantsForJobQuery(long JobId) : PagedQuery, IRequest<Result<PagedResult<ApplicationDto>>>
{
    public string RequestingUserId { get; init; } = string.Empty;
    public ApplicationStatus? Status { get; init; }
}