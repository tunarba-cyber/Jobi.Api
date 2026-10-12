using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize]
public class InterviewsController : Controller
{
    private readonly IJobiApiClient _api;
    public InterviewsController(IJobiApiClient api) => _api = api;

    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> My(CancellationToken ct) => View(await _api.GetMyInterviewsAsync(ct));

    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Manage(CancellationToken ct) => View(await _api.GetEmployerInterviewsAsync(ct));

    [Authorize(Roles = "Employer"), HttpGet]
    public IActionResult Schedule(long applicationId, string? candidate, string? job) =>
        View(new ScheduleInterviewViewModel { ApplicationId = applicationId, CandidateName = candidate, JobTitle = job });

    [Authorize(Roles = "Employer"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(ScheduleInterviewViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.ScheduleInterviewAsync(new ScheduleInterviewRequest(
            model.ApplicationId, model.ScheduledAtUtc!.Value, model.DurationMinutes, (int)model.Mode,
            model.LocationOrLink.Trim(), model.Notes), ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(model);
        }

        TempData[FlashKeys.Success] = "Interview scheduled.";
        return RedirectToAction(nameof(Manage));
    }

    [Authorize(Roles = "Candidate"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(long id, bool accept, CancellationToken ct)
    {
        var result = await _api.RespondInterviewAsync(id, accept, ct);
        if (result.Success) TempData[FlashKeys.Success] = accept ? "Interview confirmed." : "Interview declined.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;
        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Employer"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id, CancellationToken ct)
    {
        var result = await _api.CancelInterviewAsync(id, ct);
        if (result.Success) TempData[FlashKeys.Success] = "Interview cancelled.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;
        return RedirectToAction(nameof(Manage));
    }
}