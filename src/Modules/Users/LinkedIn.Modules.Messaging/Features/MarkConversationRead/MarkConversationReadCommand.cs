using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.MarkConversationRead;

public sealed record MarkConversationReadCommand(long ConversationId, string RequestingUserId) : IRequest<Result>;