namespace LinkedIn.Modules.Jobs.Features.Companies.Dtos;

public sealed record CompanyDto(
    long Id,
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    string? LogoUrl,
    DateTimeOffset CreatedAtUtc);
