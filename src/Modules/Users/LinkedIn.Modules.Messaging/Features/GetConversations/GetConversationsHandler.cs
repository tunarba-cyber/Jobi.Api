using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.GetConversations;

internal sealed class GetConversationsHandler : IRequestHandler<GetConversationsQuery, Result<List<ConversationDto>>>
{
    private readonly MessagingDbContext _db;
    public GetConversationsHandler(MessagingDbContext db) => _db = db;

    public async Task<Result<List<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken ct)
    {
        var userId = request.RequestingUserId;

        var items = await _db.Conversations
            .AsNoTracking()
            .Where(c => c.UserAId == userId || c.UserBId == userId)
            .Select(c => new ConversationDto(
                c.Id,
                c.UserAId == userId ? c.UserBId : c.UserAId,
                c.Messages.OrderByDescending(m => m.SentAtUtc).Select(m => m.Content).FirstOrDefault(),
                c.Messages.OrderByDescending(m => m.SentAtUtc).Select(m => (DateTimeOffset?)m.SentAtUtc).FirstOrDefault(),
                c.Messages.Count(m => !m.IsRead && m.SenderId != userId)))
            .ToListAsync(ct);

        return Result.Success(items);
    }
}