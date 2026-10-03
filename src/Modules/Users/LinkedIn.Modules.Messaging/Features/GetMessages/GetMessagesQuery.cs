using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.GetMessages;

public sealed record GetMessagesQuery(long ConversationId, string RequestingUserId) : IRequest<Result<List<MessageDto>>>;