using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>
/// A job category - "UI/UX Design", "Development", "Marketing".
/// Drives both the hero search dropdown and the "Most Demanding Categories"
/// cards on index.html.
/// </summary>
public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-safe identifier used in public links (/jobs?category=ui-ux-design).
    /// Lets the frontend build clean URLs without exposing database ids.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Path to the card icon, e.g. "images/icon/icon_01.svg" in the template.</summary>
    public string? IconUrl { get; set; }

    public string? Description { get; set; }

    /// <summary>Controls card order on the homepage without relying on insertion order.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Lets an admin hide a category from the site without deleting it.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Surfaces the category in the homepage's 6-card grid.</summary>
    public bool IsFeatured { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
