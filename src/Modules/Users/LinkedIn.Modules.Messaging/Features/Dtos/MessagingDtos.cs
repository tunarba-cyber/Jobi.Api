namespace LinkedIn.Modules.Messaging.Features.Dtos;


public sealed record SendMessageRequest(string RecipientUserId, string Content);