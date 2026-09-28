using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Candidate")]
public class ApplicationController : Controller
{
    private readonly IJobiApiClient _api;

    public ApplicationController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> My(int page = 1, CancellationToken ct = default)
    {
        var applications = await _api.GetMyApplicationsAsync(page < 1 ? 1 : page, 10, ct);
        return View(new MyApplicationsViewModel { Applications = applications });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyViewModel model, CancellationToken ct)
    {
        var jobUrl = Url.Action("JobDetails", "Job", new { slug = model.JobSlug })!;

        if (!ModelState.IsValid)
        {
            TempData[FlashKeys.Error] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
            return Redirect(jobUrl);
        }

        string? resumeUrl = null;
        if (model.Resume is { Length: > 0 })
        {
            await using var stream = model.Resume.OpenReadStream();
            var upload = await _api.UploadResumeAsync(stream, model.Resume.FileName, model.Resume.ContentType, ct);
            if (!upload.Success)
            {
                TempData[FlashKeys.Error] = upload.ErrorMessage;
                return Redirect(jobUrl);
            }
            resumeUrl = upload.Value;
        }

        var result = await _api.ApplyAsync(new ApplyRequest(model.JobId, resumeUrl, model.CoverLetter), ct);
        if (!result.Success)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage;
            return Redirect(jobUrl);
        }

        TempData[FlashKeys.Success] = "Your application has been submitted.";
        return RedirectToAction(nameof(My));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(long id, CancellationToken ct)
    {
        var result = await _api.WithdrawApplicationAsync(id, ct);
        if (result.Success) TempData[FlashKeys.Success] = "Application withdrawn.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(My));
    }
}