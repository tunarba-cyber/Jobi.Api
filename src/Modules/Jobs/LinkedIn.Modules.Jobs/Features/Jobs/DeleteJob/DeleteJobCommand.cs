using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.DeleteJob;

/// <summary>RequestingUserId is set by the endpoint from the caller's JWT, same as CreateJobCommand.</summary>
public sealed record DeleteJobCommand(long Id, string RequestingUserId) : IRequest<Result>;
