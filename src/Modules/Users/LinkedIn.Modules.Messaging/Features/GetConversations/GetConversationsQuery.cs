using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.GetConversations;

public sealed record GetConversationsQuery(string RequestingUserId) : IRequest<Result<List<ConversationDto>>>;