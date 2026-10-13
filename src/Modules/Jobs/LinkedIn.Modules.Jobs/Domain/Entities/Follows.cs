using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

public sealed class UserFollow : BaseEntity
{
    public string FollowerUserId { get; set; } = string.Empty;
    public string FollowedUserId { get; set; } = string.Empty;
}

public sealed class CompanyFollow : BaseEntity
{
    public string FollowerUserId { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public Company? Company { get; set; }
}