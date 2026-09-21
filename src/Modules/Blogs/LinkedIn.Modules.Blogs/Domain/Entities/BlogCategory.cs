namespace LinkedIn.Modules.Blogs.Domain.Entities;

/// <summary>Drives the badge on the blog card ("SOLUTION", "MARKETING", ...).</summary>
public class BlogCategory
{
    private BlogCategory() { } // EF

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    private readonly List<Blog> _blogs = new();
    public IReadOnlyCollection<Blog> Blogs => _blogs;

    public static BlogCategory Create(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        Slug = slug.Trim().ToLowerInvariant()
    };

    public void Rename(string name, string slug)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
    }
}
