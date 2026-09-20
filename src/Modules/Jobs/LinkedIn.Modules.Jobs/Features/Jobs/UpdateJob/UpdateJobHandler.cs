using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Categories;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Jobs.UpdateJob;

internal sealed class UpdateJobHandler : IRequestHandler<UpdateJobCommand, Result<JobDto>>
{
    private readonly JobsDbContext _db;

    public UpdateJobHandler(JobsDbContext db) => _db = db;

    public async Task<Result<JobDto>> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _db.Jobs
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (job is null)
            return Result.Failure<JobDto>(JobErrors.NotFound(request.Id));

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
            return Result.Failure<JobDto>(JobErrors.CategoryNotFound(request.CategoryId));

        var title = request.Title.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(title)
            : request.Slug.Trim().ToLowerInvariant();

        // Uniqueness check must exclude the row being edited.
        if (await _db.Jobs.AnyAsync(j => j.Slug == slug && j.Id != request.Id, cancellationToken))
            return Result.Failure<JobDto>(JobErrors.SlugAlreadyExists(slug));

        job.Title = title;
        job.Slug = slug;
        job.Description = request.Description.Trim();
        job.CategoryId = request.CategoryId;
        job.CompanyName = request.CompanyName.Trim();
        job.Location = request.Location.Trim();
        job.JobType = request.JobType;
        job.ExperienceLevel = request.ExperienceLevel;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.VacancyCount = request.VacancyCount;
        job.ApplicationDeadline = request.ApplicationDeadline;
        job.Status = request.Status;
        job.IsFeatured = request.IsFeatured;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(job.ToJobDto(category.Name));
    }
}
