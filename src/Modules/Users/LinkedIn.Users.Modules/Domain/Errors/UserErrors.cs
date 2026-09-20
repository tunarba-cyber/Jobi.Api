using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Users.Domain.Errors;

public static class UserErrors
{
    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("User.EmailExists", "An account with this email already exists.");

    // Deliberately identical wording/status for "wrong password" and "no such
    // user" - distinguishing them tells an attacker which emails are registered.
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("User.InvalidCredentials", "Email or password is incorrect.");

    public static readonly Error EmailNotConfirmed =
        Error.Unauthorized("User.EmailNotConfirmed", "Please confirm your email before logging in.");

    public static readonly Error AccountLockedOut =
        Error.Unauthorized("User.LockedOut", "Too many failed attempts. Try again later.");

    public static readonly Error InvalidOrExpiredToken =
        Error.Unauthorized("User.InvalidToken", "This link is invalid or has expired.");

    public static Error RegistrationFailed(string details) =>
        Error.Failure("User.RegistrationFailed", details);

    public static Error PasswordResetFailed(string details) =>
        Error.Failure("User.PasswordResetFailed", details);
}
