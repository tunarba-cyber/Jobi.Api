using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Jobs.Features.JobAlerts;

/// <summary>
/// Every few minutes, finds recently active jobs that match candidates' saved alerts and notifies them.
/// Idempotent: JobAlertMatches records each (alert, job) pair, so restarts and re-runs never double-notify.
/// </summary>
internal sealed class JobAlertMatcher : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Window = TimeSpan.FromDays(3);

    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<JobAlertMatcher> _logger;

    public JobAlertMatcher(IServiceScopeFactory scopes, ILogger<JobAlertMatcher> logger) { _scopes = scopes; _logger = logger; }

    private sealed record JobRow(long Id, string Title, string Slug, long CategoryId, string Location,
        string Description, string CompanyName, DateTimeOffset ActiveAt);

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(45), stop);
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try { await RunOnceAsync(stop); }
                catch (Exception ex) when (ex is not OperationCanceledException)
                { _logger.LogError(ex, "Job alert matching failed; will retry next cycle."); }
            }
            while (await timer.WaitForNextTickAsync(stop));
        }
        catch (OperationCanceledException) { }
    }

    internal async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
        var notify = scope.ServiceProvider.GetRequiredService<INotificationPublisher>();

        var since = DateTimeOffset.UtcNow - Window;
        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.Status == JobStatus.Active && (j.ModifiedAtUtc ?? j.CreatedAtUtc) >= since)
            .OrderByDescending(j => j.CreatedAtUtc).Take(500)
            .Select(j => new JobRow(j.Id, j.Title, j.Slug, j.CategoryId, j.Location, j.Description,
                j.Company!.Name, j.ModifiedAtUtc ?? j.CreatedAtUtc))
            .ToListAsync(ct);
        if (jobs.Count == 0) return;

        var alerts = await db.JobAlerts.AsNoTracking().Where(a => a.IsActive).Take(5000).ToListAsync(ct);
        if (alerts.Count == 0) return;

        var jobIds = jobs.Select(j => j.Id).ToList();
        var done = (await db.JobAlertMatches.AsNoTracking().Where(m => jobIds.Contains(m.JobId))
                .Select(m => new { m.AlertId, m.JobId }).ToListAsync(ct))
            .Select(m => (m.AlertId, m.JobId)).ToHashSet();

        var perUser = new Dictionary<string, Dictionary<long, JobRow>>();
        foreach (var alert in alerts)
            foreach (var job in jobs)
            {
                // Only jobs that appeared after the alert was created, and not already notified.
                if (job.ActiveAt < alert.CreatedAtUtc || done.Contains((alert.Id, job.Id)) || !Matches(alert, job)) continue;

                db.JobAlertMatches.Add(new JobAlertMatch { AlertId = alert.Id, JobId = job.Id, CreatedAtUtc = DateTimeOffset.UtcNow });
                if (!perUser.TryGetValue(alert.CandidateUserId, out var set)) perUser[alert.CandidateUserId] = set = new();
                set[job.Id] = job;
            }
        if (perUser.Count == 0) return;

        await db.SaveChangesAsync(ct); // record matches first, so a failed notification can't cause repeats

        foreach (var (userId, set) in perUser)
        {
            var list = set.Values.ToList();
            var single = list.Count == 1;
            await notify.PublishAsync(userId, NotificationTypes.JobMatch,
                single ? $"New job: {list[0].Title}" : $"{list.Count} new jobs match your alerts",
                single ? $"{list[0].CompanyName} · {list[0].Location}" : string.Join(", ", list.Take(3).Select(j => j.Title)),
                single ? $"/Job/JobDetails/{list[0].Slug}" : "/Job/JobList", ct: ct);
        }
    }

    private static bool Matches(JobAlert a, JobRow j) =>
        (a.CategoryId is null || a.CategoryId == j.CategoryId)
        && (string.IsNullOrWhiteSpace(a.Location) || j.Location.Contains(a.Location, StringComparison.OrdinalIgnoreCase))
        && (string.IsNullOrWhiteSpace(a.Keyword)
            || j.Title.Contains(a.Keyword, StringComparison.OrdinalIgnoreCase)
            || j.CompanyName.Contains(a.Keyword, StringComparison.OrdinalIgnoreCase)
            || j.Description.Contains(a.Keyword, StringComparison.OrdinalIgnoreCase));
}