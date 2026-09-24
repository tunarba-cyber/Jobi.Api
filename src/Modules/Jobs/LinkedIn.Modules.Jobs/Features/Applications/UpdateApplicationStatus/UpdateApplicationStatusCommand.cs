using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Applications.UpdateApplicationStatus;

public sealed record UpdateApplicationStatusCommand(
    long ApplicationId,
    ApplicationStatus NewStatus,
    string RequestingUserId) : IRequest<Result<ApplicationDto>>;