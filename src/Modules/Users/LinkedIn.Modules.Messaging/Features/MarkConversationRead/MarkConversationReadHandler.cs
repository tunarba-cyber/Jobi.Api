using LinkedIn.Modules.Messaging.Domain.Errors;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Modules.Messaging.Infrastructure.Realtime;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.MarkConversationRead;

internal sealed class MarkConversationReadHandler : IRequestHandler<MarkConversationReadCommand, Result>
{
    private readonly MessagingDbContext _db;
    private readonly IMessageNotifier _notifier;
    private readonly TimeProvider _timeProvider;

    public MarkConversationReadHandler(MessagingDbContext db, IMessageNotifier notifier, TimeProvider timeProvider)
    {
        _db = db;
        _notifier = notifier;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _db.Conversations.AsNoTracking().FirstOrDefaultAsync(
            c => c.Id == request.ConversationId &&
                 (c.UserAId == request.UserId || c.UserBId == request.UserId),
            cancellationToken);

        if (conversation is null)
            return Result.Failure(MessagingErrors.ConversationNotFound);

        var now = _timeProvider.GetUtcNow();

        // Only messages the OTHER person sent to me, single UPDATE statement.
        var updated = await _db.Messages
            .Where(m => m.ConversationId == request.ConversationId
                        && m.SenderId != request.UserId
                        && m.ReadAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ReadAtUtc, now), cancellationToken);

        if (updated > 0)
        {
            try
            {
                await _notifier.MessagesReadAsync(
                    conversation.OtherParticipant(request.UserId),
                    conversation.Id,
                    request.UserId,
                    cancellationToken);
            }
            catch { /* best-effort push */ }
        }

        return Result.Success();
    }
}
