using LinkedIn.Modules.Messaging.Domain.Entities;
using LinkedIn.Modules.Messaging.Domain.Errors;
using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Modules.Messaging.Infrastructure.Realtime;
using A=LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

internal sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    private readonly MessagingDbContext _db;
    private readonly IMessageNotifier _notifier;
    private readonly A.IUserDirectory _users;
    private readonly A.INotificationPublisher _notify;

    public SendMessageHandler(MessagingDbContext db, IMessageNotifier notifier, A.IUserDirectory users, A.INotificationPublisher notify)
    {
        _db = db;
        _notifier = notifier;
        _users = users;
        _notify = notify;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        if (request.RecipientUserId == request.RequestingUserId)
            return Result.Failure<MessageDto>(MessagingErrors.CannotMessageSelf);

        if (!await _users.ExistsAsync(request.RecipientUserId, ct))
            return Result.Failure<MessageDto>(MessagingErrors.RecipientNotFound);

        var conversation = await _db.Conversations.FirstOrDefaultAsync(c =>
            (c.UserAId == request.RequestingUserId && c.UserBId == request.RecipientUserId) ||
            (c.UserAId == request.RecipientUserId && c.UserBId == request.RequestingUserId), ct);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                UserAId = request.RequestingUserId,
                UserBId = request.RecipientUserId,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync(ct);
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = request.RequestingUserId,
            Content = request.Content.Trim(),
            SentAtUtc = DateTimeOffset.UtcNow
        };
        _db.Messages.Add(message);
        await _db.SaveChangesAsync(ct);

        var dto = new MessageDto(message.Id, message.SenderId, message.Content, message.IsRead, message.SentAtUtc);
        await _notifier.NotifyNewMessageAsync(request.RecipientUserId, dto, ct);

        var sender = (await _users.GetSummariesAsync(new[] { request.RequestingUserId }, ct)).GetValueOrDefault(request.RequestingUserId);
        var preview = message.Content.Length > 80 ? message.Content[..80] + "…" : message.Content;
        await _notify.PublishAsync(request.RecipientUserId, A.NotificationTypes.Message, "New message",
            $"{sender?.FirstName} {sender?.LastName}: {preview}", $"/Messages/Thread/{conversation.Id}",
            groupKey: $"msg-{conversation.Id}", ct: ct);

        return Result.Success(dto);
    }
}