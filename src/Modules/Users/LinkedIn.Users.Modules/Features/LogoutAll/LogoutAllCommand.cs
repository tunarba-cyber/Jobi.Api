using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.LogoutAll;

/// <summary>Revokes every active refresh token belonging to this user - every device, every session.</summary>
public sealed record LogoutAllCommand(string UserId) : IRequest<Result>;
