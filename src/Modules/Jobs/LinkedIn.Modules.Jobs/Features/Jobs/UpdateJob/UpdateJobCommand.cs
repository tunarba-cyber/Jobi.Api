using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.UpdateJob;

public sealed record UpdateJobCommand(
    long Id,
    string Title,
    string? Slug,
    string Description,
    long CategoryId,
    string CompanyName,
    string Location,
    JobType JobType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    int VacancyCount,
    DateTimeOffset? ApplicationDeadline,
    JobStatus Status,
    bool IsFeatured) : IRequest<Result<JobDto>>;
