using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.GetConversations;

public sealed record GetConversationsQuery(string UserId, int Page = 1, int PageSize = 20)
    : IRequest<Result<IReadOnlyList<ConversationDto>>>;
