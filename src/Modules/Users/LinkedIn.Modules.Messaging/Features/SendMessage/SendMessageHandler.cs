using LinkedIn.Modules.Messaging.Domain.Entities;
using LinkedIn.Modules.Messaging.Domain.Errors;
using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Modules.Messaging.Infrastructure.Realtime;
using LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

internal sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    private readonly MessagingDbContext _db;
    private readonly IUserDirectory _users;
    private readonly IMessageNotifier _notifier;
    private readonly TimeProvider _timeProvider;

    public SendMessageHandler(
        MessagingDbContext db,
        IUserDirectory users,
        IMessageNotifier notifier,
        TimeProvider timeProvider)
    {
        _db = db;
        _users = users;
        _notifier = notifier;
        _timeProvider = timeProvider;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        if (request.SenderId == request.RecipientId)
            return Result.Failure<MessageDto>(MessagingErrors.CannotMessageSelf);

        if (!await _users.ExistsAsync(request.RecipientId, cancellationToken))
            return Result.Failure<MessageDto>(MessagingErrors.RecipientNotFound);

        var now = _timeProvider.GetUtcNow();
        var (a, b) = Conversation.OrderPair(request.SenderId, request.RecipientId);

        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(c => c.UserAId == a && c.UserBId == b, cancellationToken);

        if (conversation is null)
        {
            conversation = Conversation.Start(request.SenderId, request.RecipientId, now);
            _db.Conversations.Add(conversation);
        }

        var message = conversation.AddMessage(request.SenderId, request.Body.Trim(), now);

        // Note: two users starting the very first conversation at the same instant
        // would hit the unique index; the loser gets a DbUpdateException (retry-safe).
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto(
            message.Id, conversation.Id, message.SenderId, message.Body, message.SentAtUtc, message.ReadAtUtc);

        // Persisted first, pushed second: a failed push must never lose a message.
        try { await _notifier.MessageReceivedAsync(request.RecipientId, dto, cancellationToken); }
        catch { /* recipient will fetch it via REST on next load */ }

        return Result.Success(dto);
    }
}
