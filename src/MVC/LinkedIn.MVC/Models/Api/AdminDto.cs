namespace LinkedIn.MVC.Models.Api;

public sealed record AdminUserDto(string Id, string? Email, string FirstName, string LastName, UserRole Role, bool IsSuspended, bool EmailConfirmed, DateTimeOffset CreatedAtUtc);
public sealed record ContactMessageDto(long Id, string Name, string Email, string? Subject, string Body, bool IsRead, DateTimeOffset CreatedAtUtc);
public sealed record AdminCompanyDto(long Id, string Name, string Slug, string OwnerUserId, string? WebsiteUrl, int JobCount, DateTimeOffset CreatedAtUtc);