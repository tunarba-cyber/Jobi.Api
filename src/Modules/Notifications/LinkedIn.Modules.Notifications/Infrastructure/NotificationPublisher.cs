using System.Net;
using LinkedIn.Modules.Notifications.Domain;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Notifications.Infrastructure;

internal sealed class NotificationPublisher : INotificationPublisher
{
    // Only these also go out by email. Messages and follows stay in-app.
    private static readonly HashSet<string> Emailed = new()
    {
        NotificationTypes.NewApplication, NotificationTypes.ApplicationStatus,
        NotificationTypes.Interview, NotificationTypes.JobMatch
    };

    private readonly NotificationsDbContext _db;
    private readonly EmailQueue _emails;
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationPublisher> _logger;

    public NotificationPublisher(NotificationsDbContext db, EmailQueue emails, IConfiguration config, ILogger<NotificationPublisher> logger)
    { _db = db; _emails = emails; _config = config; _logger = logger; }

    public async Task PublishAsync(string userId, string type, string title, string? body, string? link,
        string? groupKey = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        try
        {
            title = Cut(title, 200)!;
            body = Cut(body, 500);
            link = Cut(link, 300);
            var now = DateTimeOffset.UtcNow;

            if (groupKey is not null)
            {
                var existing = await _db.Notifications
                    .FirstOrDefaultAsync(n => n.UserId == userId && n.GroupKey == groupKey && !n.IsRead, ct);
                if (existing is not null)
                {
                    existing.Title = title; existing.Body = body; existing.CreatedAtUtc = now;
                    await _db.SaveChangesAsync(ct);
                    return; // merged into an unread one: no second email
                }
            }

            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                Link = link,
                GroupKey = groupKey,
                CreatedAtUtc = now
            });
            await _db.SaveChangesAsync(ct);

            if (Emailed.Contains(type))
                _emails.Enqueue(new EmailJob(userId, title, BuildEmail(title, body, link)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to publish {Type} notification for {UserId}.", type, userId);
        }
    }

    private string BuildEmail(string title, string? body, string? link)
    {
        var html = $"<h3>{WebUtility.HtmlEncode(title)}</h3>";
        if (!string.IsNullOrWhiteSpace(body)) html += $"<p>{WebUtility.HtmlEncode(body)}</p>";

        var baseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(baseUrl) && link is not null && link.StartsWith('/'))
            html += $"<p><a href=\"{WebUtility.HtmlEncode(baseUrl + link)}\">Open in Jobi</a></p>";

        return html;
    }

    private static string? Cut(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];
}