using LinkedIn.Modules.Messaging.Domain.Errors;
using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.GetMessages;

internal sealed class GetMessagesHandler : IRequestHandler<GetMessagesQuery, Result<IReadOnlyList<MessageDto>>>
{
    private readonly MessagingDbContext _db;
    public GetMessagesHandler(MessagingDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<MessageDto>>> Handle(
        GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var isParticipant = await _db.Conversations.AsNoTracking().AnyAsync(
            c => c.Id == request.ConversationId &&
                 (c.UserAId == request.UserId || c.UserBId == request.UserId),
            cancellationToken);

        if (!isParticipant)
            return Result.Failure<IReadOnlyList<MessageDto>>(MessagingErrors.ConversationNotFound);

        var query = _db.Messages.AsNoTracking().Where(m => m.ConversationId == request.ConversationId);

        if (request.Before is { } before)
            query = query.Where(m => m.SentAtUtc < before);

        IReadOnlyList<MessageDto> messages = await query
            .OrderByDescending(m => m.SentAtUtc)
            .Take(request.PageSize)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderId, m.Body, m.SentAtUtc, m.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success(messages);
    }
}
