using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Candidates.GetCandidateBySlug;

public sealed record GetCandidateBySlugQuery(string Slug) : IRequest<Result<CandidateProfileDto>>;