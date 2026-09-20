using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.ConfirmEmail;

public sealed record ConfirmEmailCommand(string UserId, string Token) : IRequest<Result>;
