namespace LinkedIn.MVC.Models.Api;

public sealed record NotificationDto(long Id, string Type, string Title, string? Body, string? Link, bool IsRead, DateTimeOffset CreatedAtUtc);
public sealed record UnreadCountDto(int Count);