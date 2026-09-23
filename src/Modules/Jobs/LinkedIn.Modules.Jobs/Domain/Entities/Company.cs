using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>
/// An employer's company profile. OwnerUserId is a plain string reference to
/// AppUser.Id in the Users module - deliberately NOT a database foreign key,
/// since Company lives in a different schema/DbContext than AppUser and this
/// module should not need to know Users' internal table structure to validate
/// it. Ownership is checked in application code (see CreateJobHandler /
/// UpdateJobHandler), not enforced by the database.
/// </summary>
public sealed class Company : BaseEntity
{
    public string OwnerUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
