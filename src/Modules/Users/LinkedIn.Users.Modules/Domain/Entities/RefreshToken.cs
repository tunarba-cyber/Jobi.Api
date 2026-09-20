using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Users.Domain.Entities;

/// <summary>
/// Only a HASH of the token is stored (same principle as password hashing) -
/// if the database ever leaks, stored rows cannot be replayed as live tokens.
/// Rotation-on-use: RefreshTokenHandler revokes the row it consumes and issues
/// a brand new one rather than letting the same refresh token be reused
/// indefinitely, so a leaked-then-reused token is detectable (the legitimate
/// user's next refresh attempt fails because their token was already revoked
/// by someone else's use of it).
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public string AppUserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }

    /// <summary>Set when this token was consumed by a refresh call and replaced.</summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTimeOffset.UtcNow;
}
