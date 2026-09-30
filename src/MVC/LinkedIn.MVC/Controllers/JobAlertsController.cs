using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Candidate")]
public class JobAlertsController : Controller
{
    private readonly IJobiApiClient _api;

    public JobAlertsController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var alerts = await _api.GetMyJobAlertsAsync(ct);
        var categories = (await _api.GetCategoriesAsync(onlyFeatured: false, page: 1, pageSize: 50, ct)).Items;
        return View(new JobAlertsViewModel { Alerts = alerts, Categories = categories });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobAlertFormViewModel model, CancellationToken ct)
    {
        var result = await _api.CreateJobAlertAsync(
            new UpsertJobAlertRequest(model.Keyword, model.CategoryId, model.Location, true), ct);

        if (result.Success) TempData[FlashKeys.Success] = "Job alert created.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(long id, bool isActive, string? keyword, long? categoryId, string? location, CancellationToken ct)
    {
        // The API's update is a full PUT - resend the alert's own fields with just IsActive flipped.
        var result = await _api.UpdateJobAlertAsync(id, new UpsertJobAlertRequest(keyword, categoryId, location, isActive), ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _api.DeleteJobAlertAsync(id, ct);
        if (result.Success) TempData[FlashKeys.Success] = "Job alert deleted.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }
}