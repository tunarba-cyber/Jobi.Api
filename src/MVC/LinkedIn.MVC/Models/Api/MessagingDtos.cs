namespace LinkedIn.MVC.Models.Api;

public sealed record ConversationDto(long Id, string OtherUserId, string OtherUserName, string? LastMessage, DateTimeOffset? LastMessageAtUtc, int UnreadCount);
public sealed record MessageDto(long Id, string SenderId, string Content, bool IsRead, DateTimeOffset SentAtUtc);
public sealed record UserSummaryDto(string Id, string FirstName, string LastName);
public sealed record SendMessageRequest(string RecipientUserId, string Content);