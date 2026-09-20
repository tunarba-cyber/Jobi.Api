using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.DeleteJob;

public sealed record DeleteJobCommand(long Id) : IRequest<Result>;
