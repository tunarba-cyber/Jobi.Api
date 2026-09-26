namespace LinkedIn.Modules.Jobs.Features.SavedJobs.Dtos;

public sealed record SavedJobDto(
    long Id,
    long JobId,
    string JobTitle,
    string CompanyName,
    DateTimeOffset SavedAtUtc);