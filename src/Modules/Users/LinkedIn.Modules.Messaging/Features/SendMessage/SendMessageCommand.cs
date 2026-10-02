using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

public sealed record SendMessageCommand(string SenderId, string RecipientId, string Body)
    : IRequest<Result<MessageDto>>;
