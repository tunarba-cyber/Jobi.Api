using LinkedIn.Modules.Messaging.Domain.Errors;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.MarkConversationRead;

internal sealed class MarkConversationReadHandler : IRequestHandler<MarkConversationReadCommand, Result>
{
    private readonly MessagingDbContext _db;
    public MarkConversationReadHandler(MessagingDbContext db) => _db = db;

    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken ct)
    {
        var owns = await _db.Conversations.AnyAsync(c =>
            c.Id == request.ConversationId && (c.UserAId == request.RequestingUserId || c.UserBId == request.RequestingUserId), ct);

        if (!owns)
            return Result.Failure(MessagingErrors.NotParticipant);

        var unread = await _db.Messages
            .Where(m => m.ConversationId == request.ConversationId && !m.IsRead && m.SenderId != request.RequestingUserId)
            .ToListAsync(ct);

        foreach (var m in unread) m.IsRead = true;
        if (unread.Count > 0) await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}