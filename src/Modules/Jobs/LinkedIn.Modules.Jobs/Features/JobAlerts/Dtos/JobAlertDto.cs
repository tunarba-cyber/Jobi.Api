namespace LinkedIn.Modules.Jobs.Features.JobAlerts.Dtos;

public sealed record JobAlertDto(
    long Id,
    string? Keyword,
    long? CategoryId,
    string? Location,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);

public sealed record UpsertJobAlertRequest(string? Keyword, long? CategoryId, string? Location, bool IsActive);