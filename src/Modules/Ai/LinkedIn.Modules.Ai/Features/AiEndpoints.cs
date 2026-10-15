using System.Security.Claims;
using LinkedIn.Modules.Ai.Domain;
using LinkedIn.Modules.Ai.Infrastructure;
using LinkedIn.Shared.Abstractions.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Ai.Features;

public sealed record JobDescriptionRequest(string Title, string? CompanyName, string? Location, string? JobType, string? ExperienceLevel, string? KeyPoints, string? Language);
public sealed record CoverLetterRequest(string JobTitle, string? CompanyName, string? JobDescription, string Background, string? Language);
public sealed record ImproveBioRequest(string Bio, string? Language);
public sealed record AiTextResponse(string Text);

internal static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ai").WithTags("AI").RequireRateLimiting(RateLimitPolicies.Ai);

        group.MapPost("/job-description", JobDescription).RequireAuthorization(AuthPolicies.Employer);
        group.MapPost("/cover-letter", CoverLetter).RequireAuthorization(AuthPolicies.Candidate);
        group.MapPost("/improve-bio", ImproveBio).RequireAuthorization(AuthPolicies.Candidate);
    }

    private static Task<IResult> JobDescription(JobDescriptionRequest r, ClaimsPrincipal user, IAiClient ai, DailyQuota quota, CancellationToken ct)
    {
        if (FirstError(Check("Title", r.Title, 200), Check("Company", r.CompanyName, 200, false), Check("Location", r.Location, 200, false),
                       Check("Job type", r.JobType, 50, false), Check("Experience level", r.ExperienceLevel, 50, false),
                       Check("Key points", r.KeyPoints, 1500, false)) is { } bad)
            return Task.FromResult(bad);

        var prompt = AiPrompts.Input(("Job title", r.Title), ("Company", r.CompanyName), ("Location", r.Location),
                                     ("Job type", r.JobType), ("Experience level", r.ExperienceLevel), ("Key points", r.KeyPoints));
        return Run(user, ai, quota, AiPrompts.JobDescription(LanguageName(r.Language)), prompt, 900, ct);
    }

    private static Task<IResult> CoverLetter(CoverLetterRequest r, ClaimsPrincipal user, IAiClient ai, DailyQuota quota, CancellationToken ct)
    {
        if (FirstError(Check("Job title", r.JobTitle, 200), Check("Company", r.CompanyName, 200, false),
                       Check("Job description", r.JobDescription, 4000, false), Check("Background", r.Background, 2000)) is { } bad)
            return Task.FromResult(bad);

        var prompt = AiPrompts.Input(("Job title", r.JobTitle), ("Company", r.CompanyName),
                                     ("Job description", r.JobDescription), ("Candidate background", r.Background));
        return Run(user, ai, quota, AiPrompts.CoverLetter(LanguageName(r.Language)), prompt, 700, ct);
    }

    private static Task<IResult> ImproveBio(ImproveBioRequest r, ClaimsPrincipal user, IAiClient ai, DailyQuota quota, CancellationToken ct)
    {
        if (FirstError(Check("Bio", r.Bio, 2000)) is { } bad) return Task.FromResult(bad);

        return Run(user, ai, quota, AiPrompts.ImproveBio(LanguageName(r.Language)), AiPrompts.Input(("Bio", r.Bio)), 300, ct);
    }

    private static async Task<IResult> Run(ClaimsPrincipal user, IAiClient ai, DailyQuota quota, string system, string prompt, int maxTokens, CancellationToken ct)
    {
        if (!ai.IsConfigured) return Problem(AiErrors.NotConfigured);

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!quota.TryConsume(userId)) return Problem(AiErrors.DailyLimit);

        var result = await ai.CompleteAsync(system, prompt, maxTokens, ct);
        return result.IsSuccess ? Results.Ok(new AiTextResponse(result.Value)) : Problem(result.Error);
    }

    private static string LanguageName(string? code) => code?.ToLowerInvariant() switch
    {
        "az" => "Azerbaijani",
        "tr" => "Turkish",
        "ru" => "Russian",
        _ => "English"
    };

    private static string? Check(string name, string? value, int max, bool required = true) =>
        string.IsNullOrWhiteSpace(value)
            ? (required ? $"{name} is required." : null)
            : value.Length > max ? $"{name} must be {max} characters or fewer." : null;

    private static IResult? FirstError(params string?[] errors) =>
        errors.FirstOrDefault(e => e is not null) is { } first
            ? Results.Problem(title: "Ai.Invalid", detail: first, statusCode: StatusCodes.Status400BadRequest)
            : null;

    private static IResult Problem(Error e) => Results.Problem(title: e.Code, detail: e.Description, statusCode: e.Code switch
    {
        "Ai.NotConfigured" or "Ai.Busy" => StatusCodes.Status503ServiceUnavailable,
        "Ai.DailyLimit" => StatusCodes.Status429TooManyRequests,
        _ => StatusCodes.Status502BadGateway
    });
}