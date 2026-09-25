using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Candidates.GetMyProfile;

public sealed record GetMyProfileQuery(string RequestingUserId) : IRequest<Result<CandidateProfileDto>>;