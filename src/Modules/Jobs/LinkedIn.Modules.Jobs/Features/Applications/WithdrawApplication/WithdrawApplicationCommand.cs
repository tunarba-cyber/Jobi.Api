using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Applications.WithdrawApplication;

public sealed record WithdrawApplicationCommand(long ApplicationId, string RequestingUserId) : IRequest<Result>;