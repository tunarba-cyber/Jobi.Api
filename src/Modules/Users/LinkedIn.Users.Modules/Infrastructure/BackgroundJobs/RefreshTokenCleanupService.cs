using LinkedIn.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Users.Infrastructure.BackgroundJobs;

/// <summary>
/// Refresh token rows accumulate forever otherwise - every login and every
/// refresh adds one, revoked or expired rows are never deleted anywhere else.
/// This runs once a day and hard-deletes anything that has been expired or
/// revoked for more than 30 days - long past being useful even for the
/// reuse-detection logging in RefreshTokenHandler, which only matters for
/// recently-revoked tokens.
/// </summary>
public sealed class RefreshTokenCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Stagger the first run slightly past startup rather than competing
        // with everything else initializing at once.
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A failed cleanup pass should never crash the app - just log
                // and try again on the next interval.
                _logger.LogError(ex, "Refresh token cleanup pass failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        var cutoff = _timeProvider.GetUtcNow() - Retention;

        var deleted = await db.RefreshTokens
            .Where(t => t.ExpiresAtUtc < cutoff || (t.RevokedAtUtc != null && t.RevokedAtUtc < cutoff))
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            _logger.LogInformation("Refresh token cleanup removed {Count} expired/revoked rows.", deleted);
    }
}
