using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Employer")]
public class SavedCandidatesController : AppController
{
    private readonly IJobiApiClient _api;

    public SavedCandidatesController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new SavedCandidatesViewModel { Items = await _api.GetSavedCandidatesAsync(ct) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(long candidateProfileId, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.SaveCandidateAsync(candidateProfileId, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsave(long candidateProfileId, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.UnsaveCandidateAsync(candidateProfileId, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }
}