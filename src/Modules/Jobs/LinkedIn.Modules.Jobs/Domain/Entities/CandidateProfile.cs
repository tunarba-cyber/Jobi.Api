using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>
/// A candidate's public profile - powers candidates-v*.html (the browsable
/// directory) and candidate-profile-v*.html (single profile view). OwnerUserId
/// is a plain string reference to AppUser.Id, same pattern as Company.OwnerUserId
/// - no database FK across modules, ownership checked in application code.
/// </summary>
public sealed class CandidateProfile : BaseEntity
{
    public string OwnerUserId { get; set; } = string.Empty;

    /// <summary>URL-safe identifier, e.g. candidate-profile-v1.html?slug=jane-doe.</summary>
    public string Slug { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>e.g. "Senior UI/UX Designer" - shown under the name on directory cards.</summary>
    public string Headline { get; set; } = string.Empty;

    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }
    public string? ResumeUrl { get; set; }

    /// <summary>Comma-separated for simplicity (e.g. "Figma,React,TypeScript") - a full tagging table isn't worth the complexity right now.</summary>
    public string? Skills { get; set; }

    public ExperienceLevel ExperienceLevel { get; set; }

    /// <summary>Toggled off to hide from the public directory/search without deleting the profile.</summary>
    public bool IsAvailableForWork { get; set; } = true;
}