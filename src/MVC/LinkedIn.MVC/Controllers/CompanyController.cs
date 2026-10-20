using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public class CompanyController : Controller
{
    private readonly IJobiApiClient _api;

    public CompanyController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken ct = default) =>
        View(new CompaniesListViewModel { Companies = await _api.GetCompaniesAsync(q, page < 1 ? 1 : page, ct), Search = q });

    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToAction(nameof(Index));

        var company = await _api.GetCompanyProfileAsync(slug, ct);
        if (company is null) return NotFound();

        var jobs = await _api.GetJobsAsync(new JobSearchRequest { CompanySlug = slug, PageSize = 6 }, ct);
        return View(new CompanyDetailsViewModel { Company = company, Jobs = jobs });
    }
}