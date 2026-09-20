using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.Register;

/// <summary>
/// Returns no tokens - registration does NOT log the user in. They must
/// confirm their email first (see ConfirmEmail), then log in separately.
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role) : IRequest<Result>;
