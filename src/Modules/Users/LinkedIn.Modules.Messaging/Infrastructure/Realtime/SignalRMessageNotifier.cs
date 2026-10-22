using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

internal sealed class SignalRMessageNotifier : IMessageNotifier
{
    private readonly IHubContext<ChatHub> _hub;
    private readonly ILogger<SignalRMessageNotifier> _logger;
    public SignalRMessageNotifier(IHubContext<ChatHub> hub, ILogger<SignalRMessageNotifier> logger) { _hub = hub; _logger = logger; }

    public async Task NotifyNewMessageAsync(string recipientUserId, object message, CancellationToken ct)
    {
        try { await _hub.Clients.User(recipientUserId).SendAsync("ReceiveMessage", message, ct); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Live delivery to {UserId} failed. The message is saved and will show on reload.", recipientUserId);
        }
    }
}