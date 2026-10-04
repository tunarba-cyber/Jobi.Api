namespace LinkedIn.Modules.Messaging.Features.Dtos;

public sealed record ConversationDto(long Id, string OtherUserId, string OtherUserName, string? LastMessage, DateTimeOffset? LastMessageAtUtc, int UnreadCount);
public sealed record MessageDto(long Id, string SenderId, string Content, bool IsRead, DateTimeOffset SentAtUtc);