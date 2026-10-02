using LinkedIn.Modules.Messaging.Features.Dtos;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

internal interface IMessageNotifier
{
    Task MessageReceivedAsync(string recipientId, MessageDto message, CancellationToken ct);
    Task MessagesReadAsync(string senderId, Guid conversationId, string readerId, CancellationToken ct);
}
