namespace LinkedIn.MVC.Models.Api;

/// <summary>Matches JobDto returned by GET /api/jobs and GET /api/jobs/{slug}.</summary>
public sealed record JobDto(
    long Id,
    string Title,
    string Slug,
    string Description,
    long CategoryId,
    string CategoryName,
    long CompanyId,
    string CompanyName,
    string CompanySlug,
    string Location,
    JobType JobType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    int VacancyCount,
    DateTimeOffset? ApplicationDeadline,
    JobStatus Status,
    bool IsFeatured,
    DateTimeOffset CreatedAtUtc)
{
    /// <summary>"$30 - $50", "$30+", or "Negotiable" - the template always shows something here.</summary>
    public string SalaryRange => (SalaryMin, SalaryMax) switch
    {
        (null, null) => "Negotiable",
        (not null, null) => $"${SalaryMin:N0}+",
        (null, not null) => $"Up to ${SalaryMax:N0}",
        _ => $"${SalaryMin:N0} - ${SalaryMax:N0}"
    };

    public string PostedAgo
    {
        get
        {
            var days = (DateTimeOffset.UtcNow - CreatedAtUtc).TotalDays;
            return days switch
            {
                < 1 => "Today",
                < 2 => "Yesterday",
                < 30 => $"{(int)days} days ago",
                < 365 => $"{(int)(days / 30)} months ago",
                _ => $"{(int)(days / 365)} years ago"
            };
        }
    }
}
