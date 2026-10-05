using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.PromoteToAdmin;

public sealed record PromoteToAdminCommand(string TargetUserId) : IRequest<Result>;