using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.CreateJob;

/// <summary>
/// Slug is optional - it is derived from Title when omitted. RequestingUserId
/// is set by the endpoint from the caller's JWT, never from the request body -
/// a client cannot post a job "as" a company it does not own.
/// </summary>
public sealed record CreateJobCommand(
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
    bool IsFeatured) : IRequest<Result<JobDto>>;
