using LinkedIn.Modules.Users.Domain.Entities;

namespace LinkedIn.Modules.Users.Infrastructure.Tokens;

public interface ITokenService
{
    /// <summary>Short-lived signed JWT. Carries user id, email, and role as claims.</summary>
    string GenerateAccessToken(AppUser user);

    /// <summary>Cryptographically random raw value - this is what the client stores.</summary>
    string GenerateRefreshTokenValue();

    /// <summary>SHA-256 hash of a raw value - this is what gets stored in the database.</summary>
    string Hash(string rawValue);
}
