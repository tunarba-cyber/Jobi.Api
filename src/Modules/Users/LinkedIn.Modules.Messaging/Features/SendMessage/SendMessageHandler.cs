using LinkedIn.Modules.Messaging.Domain.Entities;
using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Modules.Messaging.Infrastructure.Realtime;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

internal sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    private readonly MessagingDbContext _db;
    private readonly IMessageNotifier _notifier;

    public SendMessageHandler(MessagingDbContext db, IMessageNotifier notifier)
    {
        _db = db;
        _notifier = notifier;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken ct)
    {
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

        return Result.Success(dto);
    }
}