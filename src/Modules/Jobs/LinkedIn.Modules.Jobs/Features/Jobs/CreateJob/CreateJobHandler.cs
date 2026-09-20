using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Categories;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Jobs.CreateJob;

internal sealed class CreateJobHandler : IRequestHandler<CreateJobCommand, Result<JobDto>>
{
    private readonly JobsDbContext _db;

    public CreateJobHandler(JobsDbContext db) => _db = db;

    public async Task<Result<JobDto>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
            return Result.Failure<JobDto>(JobErrors.CategoryNotFound(request.CategoryId));

        // Reuses the same SlugGenerator the Categories slice uses - one slugify
        // implementation for the whole Jobs module, including the Az/Tr characters.
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(request.Title)
            : request.Slug.Trim().ToLowerInvariant();

        if (await _db.Jobs.AnyAsync(j => j.Slug == slug, cancellationToken))
            return Result.Failure<JobDto>(JobErrors.SlugAlreadyExists(slug));

        var job = new Job
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            CompanyName = request.CompanyName.Trim(),
            Location = request.Location.Trim(),
            JobType = request.JobType,
            ExperienceLevel = request.ExperienceLevel,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            VacancyCount = request.VacancyCount,
            ApplicationDeadline = request.ApplicationDeadline,
            IsFeatured = request.IsFeatured,
            Status = JobStatus.Draft // starts as Draft - a separate publish step activates it
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(job.ToJobDto(category.Name));
    }
}
