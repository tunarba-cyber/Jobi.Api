namespace LinkedIn.Modules.Jobs.Features.SavedCandidates.Dtos;

public sealed record SavedCandidateDto(
    long Id,
    long CandidateProfileId,
    string CandidateName,
    string Headline,
    DateTimeOffset SavedAtUtc);