using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.GetMessages;

/// <param name="Before">Cursor: only messages sent strictly before this instant (null = newest page).</param>
public sealed record GetMessagesQuery(string UserId, Guid ConversationId, DateTimeOffset? Before = null, int PageSize = 30)
    : IRequest<Result<IReadOnlyList<MessageDto>>>;
