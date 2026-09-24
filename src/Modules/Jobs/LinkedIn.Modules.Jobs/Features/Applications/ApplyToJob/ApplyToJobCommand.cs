using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Applications.ApplyToJob;

/// <summary>CandidateUserId/Name/Email are set by the endpoint from the JWT, never the request body.</summary>
public sealed record ApplyToJobCommand(
    long JobId,
    string CandidateUserId,
    string CandidateName,
    string CandidateEmail,
    string? ResumeUrl,
    string? CoverLetter) : IRequest<Result<ApplicationDto>>;