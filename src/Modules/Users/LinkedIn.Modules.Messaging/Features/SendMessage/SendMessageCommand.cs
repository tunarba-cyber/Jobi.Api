using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Messaging.Features.SendMessage;

public sealed record SendMessageCommand(string RequestingUserId, string RecipientUserId, string Content) : IRequest<Result<MessageDto>>;