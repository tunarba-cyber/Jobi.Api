using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.GetConversations;

internal sealed class GetConversationsHandler : IRequestHandler<GetConversationsQuery, Result<List<ConversationDto>>>
{
    private readonly MessagingDbContext _db;
    private readonly IUserDirectory _userDirectory;

    public GetConversationsHandler(MessagingDbContext db, IUserDirectory userDirectory)
    {
        _db = db;
        _userDirectory = userDirectory;
    }

    public async Task<Result<List<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken ct)
    {
        var userId = request.RequestingUserId;

        var raw = await _db.Conversations.AsNoTracking()
            .Where(c => c.UserAId == userId || c.UserBId == userId)
            .Select(c => new
            {
                c.Id,
                OtherUserId = c.UserAId == userId ? c.UserBId : c.UserAId,
                LastMessage = c.Messages.OrderByDescending(m => m.SentAtUtc).Select(m => m.Content).FirstOrDefault(),
                LastMessageAtUtc = c.Messages.OrderByDescending(m => m.SentAtUtc).Select(m => (DateTimeOffset?)m.SentAtUtc).FirstOrDefault(),
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId)
            })
            .ToListAsync(ct);

        var summaries = await _userDirectory.GetSummariesAsync(raw.Select(r => r.OtherUserId), ct);

        var items = raw.Select(r => new ConversationDto(
            r.Id,
            r.OtherUserId,
            summaries.TryGetValue(r.OtherUserId, out var s) ? $"{s.FirstName} {s.LastName}" : "Unknown user",
            r.LastMessage, r.LastMessageAtUtc, r.UnreadCount)).ToList();

        return Result.Success(items);
    }
}