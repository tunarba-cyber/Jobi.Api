using System.Threading.Channels;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Notifications.Infrastructure;

internal sealed record EmailJob(string UserId, string Subject, string Html);

internal sealed class EmailQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateBounded<EmailJob>(
        new BoundedChannelOptions(500) { FullMode = BoundedChannelFullMode.DropWrite, SingleReader = true });

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);
    public ChannelReader<EmailJob> Reader => _channel.Reader;
}

internal sealed class EmailDispatchService : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<EmailDispatchService> _logger;

    public EmailDispatchService(EmailQueue queue, IServiceScopeFactory scopes, ILogger<EmailDispatchService> logger)
    { _queue = queue; _scopes = scopes; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        try
        {
            await foreach (var job in _queue.Reader.ReadAllAsync(stop))
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<IUserMailer>()
                        .SendAsync(job.UserId, job.Subject, job.Html, stop);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Notification email to {UserId} failed.", job.UserId);
                }
            }
        }
        catch (OperationCanceledException) { }
    }
}