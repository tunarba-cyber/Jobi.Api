using LinkedIn.Modules.Users.Domain.Enums;

namespace LinkedIn.Modules.Users.Features.Dtos;

public sealed record AuthResultDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc);
