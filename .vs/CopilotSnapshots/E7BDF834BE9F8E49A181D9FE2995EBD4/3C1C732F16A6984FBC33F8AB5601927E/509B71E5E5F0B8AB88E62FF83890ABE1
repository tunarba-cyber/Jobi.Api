using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public class CandidateController : Controller
{
    private readonly IJobiApiClient _api;

    public CandidateController(IJobiApiClient api) => _api = api;

    // Public candidates directory - built in a later step.
    public async Task<IActionResult> Index(
    string? search, string? location, ExperienceLevel? experienceLevel,
    int page = 1, CancellationToken ct = default)
    {
        var filters = new CandidateSearchRequest
        {
            Search = search,
            Location = location,
            ExperienceLevel = experienceLevel,
            Page = page < 1 ? 1 : page,
            PageSize = 12
        };

        var candidates = await _api.SearchCandidatesAsync(filters, ct);
        return View(new CandidatesListViewModel
        {
            Candidates = candidates,
            Filters = filters,
            SavedCandidateIds = await LoadSavedCandidateIdsAsync(ct)
        });
    }

    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToAction(nameof(Index));

        var candidate = await _api.GetCandidateBySlugAsync(slug, ct);
        return candidate is null ? NotFound() : View(candidate);
    }
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var profile = await _api.GetMyCandidateProfileAsync(ct);

        return View(profile is null
            ? new CandidateProfileViewModel()
            : new CandidateProfileViewModel
            {
                HasProfile = true,
                FullName = profile.FullName,
                Headline = profile.Headline,
                Bio = profile.Bio,
                Location = profile.Location,
                Skills = profile.Skills,
                ExperienceLevel = profile.ExperienceLevel,
                IsAvailableForWork = profile.IsAvailableForWork,
                CurrentPhotoUrl = profile.PhotoUrl,
                CurrentResumeUrl = profile.ResumeUrl
            });
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CandidateProfileViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var existing = await _api.GetMyCandidateProfileAsync(ct);
        var photoUrl = existing?.PhotoUrl;
        var resumeUrl = existing?.ResumeUrl;

        if (model.Photo is { Length: > 0 })
        {
            await using var stream = model.Photo.OpenReadStream();
            var upload = await _api.UploadPhotoAsync(stream, model.Photo.FileName, model.Photo.ContentType, ct);
            if (!upload.Success)
            {
                ModelState.AddModelError(string.Empty, upload.ErrorMessage!);
                model.HasProfile = existing is not null;
                model.CurrentPhotoUrl = photoUrl;
                model.CurrentResumeUrl = resumeUrl;
                return View(model);
            }
            photoUrl = upload.Value;
        }

        if (model.Resume is { Length: > 0 })
        {
            await using var stream = model.Resume.OpenReadStream();
            var upload = await _api.UploadResumeAsync(stream, model.Resume.FileName, model.Resume.ContentType, ct);
            if (!upload.Success)
            {
                ModelState.AddModelError(string.Empty, upload.ErrorMessage!);
                model.HasProfile = existing is not null;
                model.CurrentPhotoUrl = photoUrl;
                model.CurrentResumeUrl = resumeUrl;
                return View(model);
            }
            resumeUrl = upload.Value;
        }

        var result = await _api.UpsertMyCandidateProfileAsync(new UpsertCandidateProfileRequest(
            model.FullName.Trim(), model.Headline.Trim(), model.Bio, model.Location,
            photoUrl, resumeUrl, model.Skills, (int)model.ExperienceLevel, model.IsAvailableForWork), ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            model.HasProfile = existing is not null;
            model.CurrentPhotoUrl = photoUrl;
            model.CurrentResumeUrl = resumeUrl;
            return View(model);
        }

        TempData[FlashKeys.Success] = existing is null ? "Profile created." : "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }
    private async Task<IReadOnlySet<long>> LoadSavedCandidateIdsAsync(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole("Employer"))
            return new HashSet<long>();

        var saved = await _api.GetSavedCandidatesAsync(ct);
        return saved.Select(s => s.CandidateProfileId).ToHashSet();
    }
}