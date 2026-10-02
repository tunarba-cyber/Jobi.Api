using LinkedIn.Modules.Messaging.Features.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

internal sealed class SignalRMessageNotifier : IMessageNotifier
{
    private readonly IHubContext<ChatHub> _hub;
    public SignalRMessageNotifier(IHubContext<ChatHub> hub) => _hub = hub;

    public Task MessageReceivedAsync(string recipientId, MessageDto message, CancellationToken ct) =>
        _hub.Clients.User(recipientId).SendAsync("MessageReceived", message, ct);

    public Task MessagesReadAsync(string senderId, Guid conversationId, string readerId, CancellationToken ct) =>
        _hub.Clients.User(senderId).SendAsync("MessagesRead", conversationId, readerId, ct);
}
