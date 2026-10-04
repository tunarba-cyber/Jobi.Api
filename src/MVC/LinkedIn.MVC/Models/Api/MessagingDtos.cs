namespace LinkedIn.MVC.Models.Api;

public sealed record ConversationDto(long Id, string OtherUserId, string? LastMessage, DateTimeOffset? LastMessageAtUtc, int UnreadCount);
public sealed record MessageDto(long Id, string SenderId, string Content, bool IsRead, DateTimeOffset SentAtUtc);
public sealed record SendMessageRequest(string RecipientUserId, string Content);