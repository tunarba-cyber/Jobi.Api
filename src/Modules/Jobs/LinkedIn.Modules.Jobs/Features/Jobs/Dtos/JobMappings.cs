using System.Linq.Expressions;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;

namespace LinkedIn.Modules.Jobs.Features.Jobs;

internal static class JobMappings
{
    /// <summary>
    /// Projection used inside EF queries. Referencing job.Category.Name here
    /// makes EF Core generate the JOIN automatically - no .Include() needed.
    /// </summary>
    public static readonly Expression<Func<Job, JobDto>> ToDto =
        job => new JobDto(
            job.Id,
            job.Title,
            job.Slug,
            job.Description,
            job.CategoryId,
            job.Category!.Name,
            job.CompanyName,
            job.Location,
            job.JobType,
            job.ExperienceLevel,
            job.SalaryMin,
            job.SalaryMax,
            job.VacancyCount,
            job.ApplicationDeadline,
            job.Status,
            job.IsFeatured,
            job.CreatedAtUtc);

    /// <summary>
    /// Used right after Create/Update, where we already hold the tracked entity
    /// in memory. categoryName is passed in explicitly (from the category we
    /// already loaded to validate CategoryId) to avoid a second round trip.
    /// </summary>
    public static JobDto ToJobDto(this Job job, string categoryName) =>
        new(job.Id,
            job.Title,
            job.Slug,
            job.Description,
            job.CategoryId,
            categoryName,
            job.CompanyName,
            job.Location,
            job.JobType,
            job.ExperienceLevel,
            job.SalaryMin,
            job.SalaryMax,
            job.VacancyCount,
            job.ApplicationDeadline,
            job.Status,
            job.IsFeatured,
            job.CreatedAtUtc);
}
