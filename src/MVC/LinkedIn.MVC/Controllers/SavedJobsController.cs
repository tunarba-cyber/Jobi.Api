using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Candidate")]
public class SavedJobsController : AppController
{
    private readonly IJobiApiClient _api;

    public SavedJobsController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new SavedJobsViewModel { Items = await _api.GetSavedJobsAsync(ct) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(long jobId, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.SaveJobAsync(jobId, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsave(long jobId, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.UnsaveJobAsync(jobId, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }
}