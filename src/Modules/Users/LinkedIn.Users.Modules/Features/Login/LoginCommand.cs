using LinkedIn.Modules.Users.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;
