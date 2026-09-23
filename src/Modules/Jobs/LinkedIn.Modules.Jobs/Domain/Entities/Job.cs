using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Shared.Abstractions.Entities;

namespace LinkedIn.Modules.Jobs.Domain.Entities;

/// <summary>
/// A single job posting - the entity everything else on the frontend
/// (index.html's featured carousel, job-list-v*.html, job-details-v*.html,
/// the employer's submit-job dashboard) ultimately points back to.
/// </summary>
public sealed class Job : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe identifier, e.g. job-details-v1.html?slug=senior-backend-engineer.</summary>
    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public long CategoryId { get; set; }
    public Category? Category { get; set; }

    public long CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Location { get; set; } = string.Empty;

    public JobType JobType { get; set; }

    public ExperienceLevel ExperienceLevel { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    /// <summary>How many openings this posting covers.</summary>
    public int VacancyCount { get; set; } = 1;

    public DateTimeOffset? ApplicationDeadline { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Draft;

    /// <summary>Surfaces the job in index.html's featured-jobs carousel.</summary>
    public bool IsFeatured { get; set; }
}
