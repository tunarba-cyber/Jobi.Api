using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Users.Infrastructure.Email;

/// <summary>
/// Local-development stand-in for a real mail provider. Writes each email as an
/// .html file under ./dev-emails/ so confirmation links and reset tokens can be
/// opened directly in a browser while testing, with no SMTP account needed.
/// Swap this for a real IEmailSender (SendGrid/SES/SMTP) before deploying -
/// nothing outside this file needs to change to do that.
/// </summary>
public sealed class DevEmailSender : IEmailSender
{
    private readonly ILogger<DevEmailSender> _logger;
    private const string OutputDirectory = "dev-emails";

    public DevEmailSender(ILogger<DevEmailSender> logger) => _logger = logger;

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(OutputDirectory);

        var safeEmail = string.Join("_", toEmail.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{safeEmail}.html";
        var path = Path.Combine(OutputDirectory, fileName);

        var content = $"<h3>To: {toEmail}</h3><h4>Subject: {subject}</h4><hr/>{htmlBody}";
        File.WriteAllText(path, content);

        _logger.LogInformation(
            "DEV EMAIL written to {Path} - To: {ToEmail}, Subject: {Subject}",
            Path.GetFullPath(path), toEmail, subject);

        return Task.CompletedTask;
    }
}
