namespace LinkedIn.MVC.Models.Api;

public sealed record CompanyListItemDto(long Id, string Name, string Slug, string? Description, string? WebsiteUrl, string? LogoUrl, int OpenJobs, int Followers, bool IsFollowing);
public sealed record CompanyProfileDto(long Id, string Name, string Slug, string? Description, string? WebsiteUrl, string? LogoUrl, int OpenJobs, int Followers, bool IsFollowing, bool IsOwner, DateTimeOffset CreatedAtUtc);
public sealed record SuggestionDto(string UserId, string Name, string Role, string Reason, int FollowerCount);
public sealed record CompanySuggestionDto(long CompanyId, string Name, string Slug, string? LogoUrl, string Reason, int FollowerCount, int OpenJobs);
public sealed record PostDto(long Id, string AuthorId, string AuthorName, string AuthorRole, string Caption, string? ImageUrl, int Likes, int Comments, bool LikedByMe, bool CanDelete, DateTimeOffset CreatedAtUtc);
public sealed record CommentDto(long Id, string UserId, string UserName, string Text, bool CanDelete, DateTimeOffset CreatedAtUtc);
public sealed record CreatePostRequest(string? Caption, string? ImageUrl);