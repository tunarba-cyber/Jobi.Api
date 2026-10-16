using System.Linq.Expressions;
using System.Security.Claims;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Interviews;

public sealed record InterviewDto(long Id, long ApplicationId, string JobTitle, string CompanyName, string CandidateName,
    DateTimeOffset ScheduledAtUtc, int DurationMinutes, InterviewMode Mode, string LocationOrLink, string? Notes, InterviewStatus Status);
public sealed record ScheduleInterviewRequest(long ApplicationId, DateTimeOffset ScheduledAtUtc, int DurationMinutes, InterviewMode Mode, string LocationOrLink, string? Notes);
public sealed record RespondInterviewRequest(bool Accept);

internal static class InterviewEndpoints
{
    private static readonly Expression<Func<Interview, InterviewDto>> ToDto = i => new InterviewDto(
        i.Id, i.ApplicationId, i.Application!.Job!.Title, i.Application.Job.Company!.Name, i.Application.CandidateName,
        i.ScheduledAtUtc, i.DurationMinutes, i.Mode, i.LocationOrLink, i.Notes, i.Status);

    public static void MapInterviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/interviews").WithTags("Interviews");
        g.MapPost("/", Schedule).RequireAuthorization("RequireEmployer");
        g.MapGet("/employer", GetForEmployer).RequireAuthorization("RequireEmployer");
        g.MapPut("/{id:long}/cancel", Cancel).RequireAuthorization("RequireEmployer");
        g.MapGet("/mine", GetMine).RequireAuthorization("RequireCandidate");
        g.MapPut("/{id:long}/respond", Respond).RequireAuthorization("RequireCandidate");
    }

    private static string Uid(ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private static IResult Problem(string title, string detail, int status) => Results.Problem(title: title, detail: detail, statusCode: status);

    private static async Task<IResult> GetForEmployer(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var uid = Uid(user);
        return Results.Ok(await db.Interviews.AsNoTracking()
            .Where(i => i.Application!.Job!.Company!.OwnerUserId == uid)
            .OrderByDescending(i => i.ScheduledAtUtc).Select(ToDto).ToListAsync(ct));
    }

    private static async Task<IResult> GetMine(ClaimsPrincipal user, JobsDbContext db, CancellationToken ct)
    {
        var uid = Uid(user);
        return Results.Ok(await db.Interviews.AsNoTracking()
            .Where(i => i.Application!.CandidateUserId == uid)
            .OrderByDescending(i => i.ScheduledAtUtc).Select(ToDto).ToListAsync(ct));
    }

    private static async Task<IResult> Schedule(ScheduleInterviewRequest r, ClaimsPrincipal user, JobsDbContext db,
        INotificationPublisher notify, CancellationToken ct)
    {
        if (r.ScheduledAtUtc <= DateTimeOffset.UtcNow.AddMinutes(5)
            || r.DurationMinutes is < 15 or > 480
            || !Enum.IsDefined(r.Mode)
            || string.IsNullOrWhiteSpace(r.LocationOrLink) || r.LocationOrLink.Length > 500
            || r.Notes?.Length > 1000)
            return Problem("Interview.Invalid", "Pick a future time, a 15-480 minute duration, and a location or link.", 400);

        var app = await db.Applications.Include(a => a.Job).ThenInclude(j => j!.Company)
            .FirstOrDefaultAsync(a => a.Id == r.ApplicationId, ct);
        if (app is null) return Problem("Application.NotFound", "Application not found.", 404);
        if (app.Job?.Company?.OwnerUserId != Uid(user)) return Results.Forbid();
        if (app.Status == ApplicationStatus.Rejected) return Problem("Interview.Rejected", "This application was rejected.", 409);

        var hasActive = await db.Interviews.AnyAsync(i => i.ApplicationId == app.Id
            && (i.Status == InterviewStatus.Scheduled || i.Status == InterviewStatus.Confirmed), ct);
        if (hasActive) return Problem("Interview.Exists", "Cancel the existing interview first.", 409);

        var interview = new Interview
        {
            ApplicationId = app.Id,
            ScheduledAtUtc = r.ScheduledAtUtc,
            DurationMinutes = r.DurationMinutes,
            Mode = r.Mode,
            LocationOrLink = r.LocationOrLink.Trim(),
            Notes = r.Notes?.Trim()
        };
        db.Interviews.Add(interview);
        if (app.Status is ApplicationStatus.Applied or ApplicationStatus.Reviewed) app.Status = ApplicationStatus.Shortlisted;
        await db.SaveChangesAsync(ct);

        await notify.PublishAsync(app.CandidateUserId, NotificationTypes.Interview, "Interview invitation",
            $"You've been invited to interview for {app.Job!.Title}. Open Interviews to confirm.", "/Interviews/My", ct: ct);

        var dto = await db.Interviews.AsNoTracking().Where(i => i.Id == interview.Id).Select(ToDto).FirstAsync(ct);
        return Results.Created($"/api/interviews/{interview.Id}", dto);
    }

    private static async Task<IResult> Cancel(long id, ClaimsPrincipal user, JobsDbContext db,
        INotificationPublisher notify, CancellationToken ct)
    {
        var i = await db.Interviews.Include(x => x.Application).ThenInclude(a => a!.Job).ThenInclude(j => j!.Company)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (i is null) return Problem("Interview.NotFound", "Interview not found.", 404);
        if (i.Application?.Job?.Company?.OwnerUserId != Uid(user)) return Results.Forbid();
        if (i.Status is InterviewStatus.Cancelled or InterviewStatus.Declined) return Results.NoContent();

        i.Status = InterviewStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        await notify.PublishAsync(i.Application!.CandidateUserId, NotificationTypes.Interview, "Interview cancelled",
            $"Your interview for {i.Application.Job!.Title} was cancelled.", "/Interviews/My", ct: ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Respond(long id, RespondInterviewRequest r, ClaimsPrincipal user, JobsDbContext db,
        INotificationPublisher notify, CancellationToken ct)
    {
        var i = await db.Interviews
            .Include(x => x.Application).ThenInclude(a => a!.Job).ThenInclude(j => j!.Company)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (i is null) return Problem("Interview.NotFound", "Interview not found.", 404);
        if (i.Application?.CandidateUserId != Uid(user)) return Results.Forbid();
        if (i.Status is not (InterviewStatus.Scheduled or InterviewStatus.Confirmed) || i.ScheduledAtUtc <= DateTimeOffset.UtcNow)
            return Problem("Interview.Closed", "This interview can no longer be changed.", 409);

        i.Status = r.Accept ? InterviewStatus.Confirmed : InterviewStatus.Declined;
        await db.SaveChangesAsync(ct);

        var ownerId = i.Application!.Job!.Company!.OwnerUserId;
        await notify.PublishAsync(ownerId, NotificationTypes.Interview,
            r.Accept ? "Interview confirmed" : "Interview declined",
            $"{i.Application.CandidateName} {(r.Accept ? "confirmed" : "declined")} the interview for {i.Application.Job.Title}.",
            "/Interviews/Manage", ct: ct);
        return Results.NoContent();
    }
}