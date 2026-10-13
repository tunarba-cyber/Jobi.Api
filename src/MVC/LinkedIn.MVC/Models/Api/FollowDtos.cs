namespace LinkedIn.MVC.Models.Api;

public sealed record FollowSummaryDto(int Followers, int Following, int FollowedCompanies, int CompanyFollowers);
public sealed record FollowUserDto(string UserId, string Name, string Role, bool IsFollowing, int FollowerCount);
public sealed record FollowCompanyDto(long CompanyId, string Name, string Slug, bool IsFollowing, int FollowerCount);