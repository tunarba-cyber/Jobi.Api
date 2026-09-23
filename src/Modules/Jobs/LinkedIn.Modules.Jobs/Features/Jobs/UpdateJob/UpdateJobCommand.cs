using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.UpdateJob;

/// <summary>RequestingUserId is set by the endpoint from the caller's JWT, same as CreateJobCommand.</summary>
public sealed record UpdateJobCommand(
    long Id,
    string RequestingUserId,
    string Title,
    string? Slug,
    string Description,
    long CategoryId,
    string Location,
    JobType JobType,
    ExperienceLevel ExperienceLevel,
    decimal? SalaryMin,
    decimal? SalaryMax,
    int VacancyCount,
    DateTimeOffset? ApplicationDeadline,
    JobStatus Status,
    bool IsFeatured) : IRequest<Result<JobDto>>;
