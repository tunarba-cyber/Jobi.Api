using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public class CompanyController : Controller
{
    private readonly IJobiApiClient _api;

    public CompanyController(IJobiApiClient api) => _api = api;

    // No GET /api/companies list endpoint exists on the backend yet - this page
    // points people at Job List, which is a real way to discover companies,
    // rather than a directory the API can't back.
    public IActionResult Index() => View();

    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToAction(nameof(Index));

        var company = await _api.GetCompanyBySlugAsync(slug, ct);
        return company is null ? NotFound() : View(new CompanyDetailsViewModel { Company = company });
    }
}