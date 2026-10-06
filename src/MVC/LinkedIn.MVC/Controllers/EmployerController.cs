using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Employer")]
public class EmployerController : Controller
{
    private readonly IJobiApiClient _api;

    public EmployerController(IJobiApiClient api) => _api = api;

    // ---- Dashboard ---------------------------------------------------------
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var company = await _api.GetMyCompanyAsync(ct);
        if (company is null)
        {
            TempData[FlashKeys.Error] = "Create your company profile before posting jobs.";
            return RedirectToAction(nameof(CompanyProfile));
        }

        var jobs = await _api.GetMyJobsAsync(page < 1 ? 1 : page, 10, ct);
        return View(new EmployerDashboardViewModel { Company = company, Jobs = jobs });
    }

    // ---- Company -----------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> CompanyProfile(CancellationToken ct)
    {
        var company = await _api.GetMyCompanyAsync(ct);
        return View(company is null
            ? new CompanyFormViewModel()
            : new CompanyFormViewModel
            {
                IsEdit = true,
                Name = company.Name,
                Description = company.Description,
                WebsiteUrl = company.WebsiteUrl,
                CurrentLogoUrl = company.LogoUrl
            });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompanyProfile(CompanyFormViewModel model, CancellationToken ct)
    {
        var existing = await _api.GetMyCompanyAsync(ct);
        model.IsEdit = existing is not null;
        model.CurrentLogoUrl = existing?.LogoUrl;
        model.ValidateLogo(ModelState);

        if (!ModelState.IsValid) return View(model);

        var logoUrl = existing?.LogoUrl;
        if (model.Logo is { Length: > 0 })
        {
            await using var stream = model.Logo.OpenReadStream();
            var upload = await _api.UploadLogoAsync(
                stream, model.Logo.FileName, model.Logo.ContentType, ct);

            if (!upload.Success)
            {
                ModelState.AddModelError(nameof(model.Logo), upload.ErrorMessage!);
                return View(model);
            }

            logoUrl = upload.Value;
            model.CurrentLogoUrl = logoUrl;
        }

        var result = existing is null
            ? await _api.CreateCompanyAsync(
                new CreateCompanyRequest(model.Name.Trim(), null, model.Description, model.WebsiteUrl, logoUrl), ct)
            : await _api.UpdateMyCompanyAsync(
                new UpdateCompanyRequest(model.Name.Trim(), model.Description, model.WebsiteUrl, logoUrl), ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(model);
        }

        TempData[FlashKeys.Success] = existing is null ? "Company profile created." : "Company profile updated.";
        return RedirectToAction(nameof(Index));
    }
    // ---- Applicants -------------------------------------------------------
    public async Task<IActionResult> Applicants(long jobId, int page = 1, CancellationToken ct = default)
    {
        var job = await _api.GetMyJobAsync(jobId, ct);
        if (job is null) return NotFound();

        var applicants = await _api.GetApplicantsForJobAsync(jobId, page < 1 ? 1 : page, 20, ct);
        return View(new ApplicantsViewModel { Job = job, Applicants = applicants });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateApplicantStatus(long applicationId, long jobId, ApplicationStatus status, CancellationToken ct)
    {
        var result = await _api.UpdateApplicationStatusAsync(applicationId, status, ct);
        if (result.Success) TempData[FlashKeys.Success] = $"Application marked as {status}.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(Applicants), new { jobId });
    }

    // ---- Post / edit a job -------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> PostJob(CancellationToken ct)
    {
        if (await _api.GetMyCompanyAsync(ct) is null)
        {
            TempData[FlashKeys.Error] = "Create your company profile before posting jobs.";
            return RedirectToAction(nameof(CompanyProfile));
        }

        return View("JobForm", new JobFormViewModel { Categories = await LoadCategoriesAsync(ct) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PostJob(JobFormViewModel model, CancellationToken ct)
    {
        if (!IsValid(model))
        {
            model.Categories = await LoadCategoriesAsync(ct);
            return View("JobForm", model);
        }

        var result = await _api.CreateJobAsync(ToRequest(model), ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            model.Categories = await LoadCategoriesAsync(ct);
            return View("JobForm", model);
        }

        TempData[FlashKeys.Success] = "Job created. Check its status in the list below.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditJob(long id, CancellationToken ct)
    {
        var job = await _api.GetMyJobAsync(id, ct);
        if (job is null) return NotFound();

        var model = ToForm(job);
        model.Categories = await LoadCategoriesAsync(ct);
        return View("JobForm", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(long id, JobFormViewModel model, CancellationToken ct)
    {
        model.Id = id;

        if (!IsValid(model))
        {
            model.Categories = await LoadCategoriesAsync(ct);
            return View("JobForm", model);
        }

        var result = await _api.UpdateJobAsync(id, ToRequest(model), ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            model.Categories = await LoadCategoriesAsync(ct);
            return View("JobForm", model);
        }

        TempData[FlashKeys.Success] = "Job updated.";
        return RedirectToAction(nameof(Index));
    }

    // ---- Publish / close / delete -----------------------------------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(long id, JobStatus status, CancellationToken ct)
    {
        // The API's update is a full PUT, so load the job and resend it with only the status changed.
        var job = await _api.GetMyJobAsync(id, ct);
        if (job is null) return NotFound();

        var result = await _api.UpdateJobAsync(id, ToRequest(job, status), ct);
        if (result.Success) TempData[FlashKeys.Success] = $"Job is now {status}.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(long id, CancellationToken ct)
    {
        var result = await _api.DeleteJobAsync(id, ct);
        if (result.Success) TempData[FlashKeys.Success] = "Job deleted.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    // ---- Helpers -----------------------------------------------------------
    private async Task<IReadOnlyList<CategoryDto>> LoadCategoriesAsync(CancellationToken ct) =>
        (await _api.GetCategoriesAsync(onlyFeatured: false, page: 1, pageSize: 50, ct: ct)).Items;

    private bool IsValid(JobFormViewModel model)
    {
        if (model.SalaryMin is { } min && model.SalaryMax is { } max && max < min)
            ModelState.AddModelError(nameof(model.SalaryMax), "Maximum salary can't be lower than the minimum.");

        return ModelState.IsValid;
    }

    // A date picked in the form means "open through the end of that day".
    private static DateTimeOffset? EndOfDayUtc(DateTime? date) =>
        date is { } d
            ? new DateTimeOffset(DateTime.SpecifyKind(d.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc))
            : null;

    private static JobWriteRequest ToRequest(JobFormViewModel m) => new(
        m.Title.Trim(), m.Slug, m.Description.Trim(), m.CategoryId, m.Location.Trim(),
        (int)m.JobType, (int)m.ExperienceLevel, m.SalaryMin, m.SalaryMax, m.VacancyCount,
        EndOfDayUtc(m.ApplicationDeadline), (int)m.Status, m.IsFeatured);

    private static JobWriteRequest ToRequest(JobDto j, JobStatus status) => new(
        j.Title, j.Slug, j.Description, j.CategoryId, j.Location,
        (int)j.JobType, (int)j.ExperienceLevel, j.SalaryMin, j.SalaryMax, j.VacancyCount,
        j.ApplicationDeadline, (int)status, j.IsFeatured);

    private static JobFormViewModel ToForm(JobDto j) => new()
    {
        Id = j.Id,
        Slug = j.Slug,
        IsFeatured = j.IsFeatured,
        Status = j.Status,
        Title = j.Title,
        Description = j.Description,
        CategoryId = j.CategoryId,
        Location = j.Location,
        JobType = j.JobType,
        ExperienceLevel = j.ExperienceLevel,
        SalaryMin = j.SalaryMin,
        SalaryMax = j.SalaryMax,
        VacancyCount = j.VacancyCount,
        ApplicationDeadline = j.ApplicationDeadline?.UtcDateTime.Date
    };
}