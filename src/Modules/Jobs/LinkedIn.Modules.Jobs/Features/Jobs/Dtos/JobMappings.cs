using System.Linq.Expressions;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;

namespace LinkedIn.Modules.Jobs.Features.Jobs;

internal static class JobMappings
{
    /// <summary>
    /// Projection used inside EF queries. Referencing job.Category.Name and
    /// job.Company.Name here makes EF Core generate the JOINs automatically -
    /// no .Include() needed.
    /// </summary>
    public static readonly Expression<Func<Job, JobDto>> ToDto =
        job => new JobDto(
            job.Id,
            job.Title,
            job.Slug,
            job.Description,
            job.CategoryId,
            job.Category!.Name,
            job.CompanyId,
            job.Company!.Name,
            job.Company!.Slug,
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
    /// in memory. categoryName/company are passed in explicitly (from the
    /// category/company already loaded to validate the command) to avoid an
    /// extra round trip.
    /// </summary>
    public static JobDto ToJobDto(this Job job, string categoryName, Company company) =>
        new(job.Id,
            job.Title,
            job.Slug,
            job.Description,
            job.CategoryId,
            categoryName,
            company.Id,
            company.Name,
            company.Slug,
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
