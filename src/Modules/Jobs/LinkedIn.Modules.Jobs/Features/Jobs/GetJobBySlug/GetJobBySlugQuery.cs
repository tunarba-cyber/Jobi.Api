using LinkedIn.Modules.Jobs.Features.Jobs.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Jobs.GetJobBySlug;

/// <summary>
/// Lookup by slug, not id, so public URLs stay readable and stable:
/// job-details-v1.html?slug=senior-backend-engineer
/// </summary>
public sealed record GetJobBySlugQuery(string Slug) : IRequest<Result<JobDto>>;
