namespace LinkedIn.MVC.Models.Api;

public enum UserRole { Candidate, Employer }

public sealed record AuthResultDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc);

public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Wraps a write-call outcome so the controller can show a real error message
/// ("invalid credentials", "email already registered") instead of the read-only
/// GetOrDefaultAsync pattern, which silently swallows failures into empty data.
/// </summary>
public sealed record ApiCallResult<T>(bool Success, T? Value, string? ErrorMessage)
{
    public static ApiCallResult<T> Ok(T value) => new(true, value, null);
    public static ApiCallResult<T> Fail(string error) => new(false, default, error);
}
public sealed record ConfirmEmailRequest(string UserId, string Token);