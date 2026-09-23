namespace LinkedIn.Modules.Users.Infrastructure.Email;

/// <summary>
/// Bound from the "Smtp" configuration section. If Host is left empty,
/// UsersModule falls back to DevEmailSender automatically - no code change
/// needed to go from "no email setup" to "real email setup", just config.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "LinkedIn";
}
