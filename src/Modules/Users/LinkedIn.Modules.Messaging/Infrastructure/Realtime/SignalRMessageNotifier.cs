using Microsoft.AspNetCore.SignalR;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

internal sealed class SignalRMessageNotifier : IMessageNotifier
{
    private readonly IHubContext<ChatHub> _hub;
    public SignalRMessageNotifier(IHubContext<ChatHub> hub) => _hub = hub;

    public Task NotifyNewMessageAsync(string recipientUserId, object message, CancellationToken ct) =>
        _hub.Clients.User(recipientUserId).SendAsync("ReceiveMessage", message, ct);
}