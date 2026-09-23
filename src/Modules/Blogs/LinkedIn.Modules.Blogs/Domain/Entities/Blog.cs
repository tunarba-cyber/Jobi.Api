namespace LinkedIn.Modules.Blogs.Domain.Entities;

public class Blog
{
    private Blog() { } // EF

    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string Summary { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public string? ImageUrl { get; private set; }

    public Guid CategoryId { get; private set; }
    public BlogCategory Category { get; private set; } = default!;

    /// <summary>Author is a user owned by the Users module — referenced by id only, no FK across modules.</summary>
    public string AuthorId { get; private set; } = default!;

    public bool IsFeatured { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public int ViewCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Blog Create(
        string title,
        string slug,
        string summary,
        string content,
        Guid categoryId,
        string authorId,
        string? imageUrl,
        bool isFeatured,
        bool publishNow)
    {
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Summary = summary.Trim(),
            Content = content,
            CategoryId = categoryId,
            AuthorId = authorId,
            ImageUrl = imageUrl,
            IsFeatured = isFeatured,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (publishNow) blog.Publish();
        return blog;
    }

    public void Update(string title, string summary, string content, Guid categoryId, string? imageUrl, bool isFeatured)
    {
        Title = title.Trim();
        Summary = summary.Trim();
        Content = content;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        IsFeatured = isFeatured;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        if (IsPublished) return;
        IsPublished = true;
        PublishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncreaseViewCount() => ViewCount++;

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}