using LinkedIn.Modules.Messaging.Domain.Errors;
using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.GetMessages;

internal sealed class GetMessagesHandler : IRequestHandler<GetMessagesQuery, Result<List<MessageDto>>>
{
    private readonly MessagingDbContext _db;
    public GetMessagesHandler(MessagingDbContext db) => _db = db;

    public async Task<Result<List<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        var owns = await _db.Conversations.AnyAsync(c =>
            c.Id == request.ConversationId && (c.UserAId == request.RequestingUserId || c.UserBId == request.RequestingUserId), ct);

        if (!owns)
            return Result.Failure<List<MessageDto>>(MessagingErrors.NotParticipant);

        var messages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.SentAtUtc)
            .Select(m => new MessageDto(m.Id, m.SenderId, m.Content, m.IsRead, m.SentAtUtc))
            .ToListAsync(ct);

        return Result.Success(messages);
    }
}