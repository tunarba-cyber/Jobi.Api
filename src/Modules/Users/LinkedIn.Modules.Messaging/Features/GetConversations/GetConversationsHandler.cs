using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Features.GetConversations;

internal sealed class GetConversationsHandler
    : IRequestHandler<GetConversationsQuery, Result<IReadOnlyList<ConversationDto>>>
{
    private readonly MessagingDbContext _db;
    private readonly IUserDirectory _users;

    public GetConversationsHandler(MessagingDbContext db, IUserDirectory users)
    {
        _db = db;
        _users = users;
    }

    public async Task<Result<IReadOnlyList<ConversationDto>>> Handle(
        GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var me = request.UserId;

        var rows = await _db.Conversations
            .AsNoTracking()
            .Where(c => c.UserAId == me || c.UserBId == me)
            .OrderByDescending(c => c.LastMessageAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new
            {
                c.Id,
                OtherId = c.UserAId == me ? c.UserBId : c.UserAId,
                c.LastMessagePreview,
                c.LastMessageAtUtc,
                Unread = c.Messages.Count(m => m.SenderId != me && m.ReadAtUtc == null)
            })
            .ToListAsync(cancellationToken);

        var people = await _users.GetSummariesAsync(rows.Select(r => r.OtherId), cancellationToken);

        IReadOnlyList<ConversationDto> result = rows
            .Select(r =>
            {
                var p = people.GetValueOrDefault(r.OtherId);
                var other = p is null
                    ? new ParticipantDto(r.OtherId, "Deleted", "User")
                    : new ParticipantDto(p.Id, p.FirstName, p.LastName);
                return new ConversationDto(r.Id, other, r.LastMessagePreview, r.LastMessageAtUtc, r.Unread);
            })
            .ToList();

        return Result.Success(result);
    }
}
