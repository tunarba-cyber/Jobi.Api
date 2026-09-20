using LinkedIn.Modules.Jobs.Domain.Enums;

namespace LinkedIn.Modules.Jobs.Features.Jobs.Dtos;

/// <summary>
/// What a job card / job-details page needs. CategoryName is denormalised here
/// so the frontend never has to make a second call just to display it.
/// </summary>
public sealed record JobDto(
    long Id,
    string Title,
    string Slug,
    string Description,
    long CategoryId,
    string CategoryName,
    string CompanyName,
    string Location,
    JobType JobType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    int VacancyCount,
    DateTimeOffset? ApplicationDeadline,
    JobStatus Status,
    bool IsFeatured,
    DateTimeOffset CreatedAtUtc);
