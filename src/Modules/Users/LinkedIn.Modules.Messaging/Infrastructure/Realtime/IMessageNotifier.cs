namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

public interface IMessageNotifier
{
    Task NotifyNewMessageAsync(string recipientUserId, object message, CancellationToken ct);
}