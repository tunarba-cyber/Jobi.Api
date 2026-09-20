using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.Logout;

/// <summary>Revokes one specific refresh token - only that session is logged out, not every device.</summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
