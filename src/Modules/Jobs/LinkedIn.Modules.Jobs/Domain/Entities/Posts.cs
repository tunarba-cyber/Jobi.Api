using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

public sealed class Post : BaseEntity
{
    public string AuthorUserId { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public sealed class PostLike : BaseEntity
{
    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public sealed class PostComment : BaseEntity
{
    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}