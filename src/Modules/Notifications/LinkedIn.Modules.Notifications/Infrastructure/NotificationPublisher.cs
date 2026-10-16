using LinkedIn.Modules.Notifications.Domain;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Notifications.Infrastructure;

internal sealed class NotificationPublisher : INotificationPublisher
{
    private readonly NotificationsDbContext _db;
    private readonly ILogger<NotificationPublisher> _logger;

    public NotificationPublisher(NotificationsDbContext db, ILogger<NotificationPublisher> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task PublishAsync(string userId, string type, string title, string? body, string? link,
        string? groupKey = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        try
        {
            title = Cut(title, 200)!;
            body = Cut(body, 500);
            var now = DateTimeOffset.UtcNow;

            if (groupKey is not null)
            {
                var existing = await _db.Notifications
                    .FirstOrDefaultAsync(n => n.UserId == userId && n.GroupKey == groupKey && !n.IsRead, ct);
                if (existing is not null)
                {
                    existing.Title = title; existing.Body = body; existing.CreatedAtUtc = now;
                    await _db.SaveChangesAsync(ct);
                    return;
                }
            }

            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                Link = Cut(link, 300),
                GroupKey = groupKey,
                CreatedAtUtc = now
            });
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to publish {Type} notification for {UserId}.", type, userId);
        }
    }

    private static string? Cut(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];
}