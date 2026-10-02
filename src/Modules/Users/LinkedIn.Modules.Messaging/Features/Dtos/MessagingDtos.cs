namespace LinkedIn.Modules.Messaging.Features.Dtos;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    string SenderId,
    string Body,
    DateTimeOffset SentAtUtc,
    DateTimeOffset? ReadAtUtc);

public sealed record ParticipantDto(string Id, string FirstName, string LastName);

public sealed record ConversationDto(
    Guid Id,
    ParticipantDto Other,
    string? LastMessagePreview,
    DateTimeOffset? LastMessageAtUtc,
    int UnreadCount);
