using LinkedIn.MVC.Models.Api;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class FollowHubViewModel
{
    public required FollowSummaryDto Summary { get; init; }
    public IReadOnlyList<FollowUserDto> Followers { get; init; } = Array.Empty<FollowUserDto>();
    public IReadOnlyList<FollowUserDto> Following { get; init; } = Array.Empty<FollowUserDto>();
    public IReadOnlyList<FollowCompanyDto> Companies { get; init; } = Array.Empty<FollowCompanyDto>();
}

public sealed class ExploreUsersViewModel
{
    public required PagedResult<FollowUserDto> Users { get; init; }
    public string? Search { get; init; }
}

public sealed class ExploreCompaniesViewModel
{
    public required PagedResult<FollowCompanyDto> Companies { get; init; }
    public string? Search { get; init; }
}

/// <summary>Kind is "User" or "Company" and picks the FollowUser/UnfollowUser/FollowCompany/... action.</summary>
public sealed record FollowButtonViewModel(string Kind, string TargetId, bool IsFollowing);