namespace LinkedIn.Modules.Users.Infrastructure.Tokens;

/// <summary>
/// Bound from the "Jwt" configuration section. SigningKey must come from
/// user-secrets/environment/a real secrets manager in every environment -
/// never committed to appsettings.json. This is the fix for the exact issue
/// flagged in the old Pronia review, where the signing key was a literal
/// string in source code.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
