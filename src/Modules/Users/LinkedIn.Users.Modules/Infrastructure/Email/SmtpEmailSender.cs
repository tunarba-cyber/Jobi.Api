using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkedIn.Modules.Users.Infrastructure.Email;

/// <summary>
/// Real email delivery via SMTP, using the framework's built-in SmtpClient -
/// no extra package needed. Works with any standard SMTP provider (SendGrid,
/// Mailgun, AWS SES, Gmail's SMTP relay, your own mail server) since they all
/// speak plain SMTP; only Host/Port/credentials change between them.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (SmtpException ex)
        {
            // Don't let a mail-provider outage take down registration/reset
            // flows entirely - log it loudly so it gets noticed and fixed,
            // but the caller (e.g. RegisterHandler) still completes.
            _logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP.", toEmail);
        }
    }
}
