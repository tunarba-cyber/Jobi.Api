using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Users.Features.ForgotPassword;

/// <summary>Always returns Success, regardless of whether the email exists - see UserErrors notes on enumeration.</summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
