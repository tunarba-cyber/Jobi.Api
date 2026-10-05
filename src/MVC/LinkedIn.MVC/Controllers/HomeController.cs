using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LinkedIn.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IJobiApiClient _api;

    public HomeController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Three calls, run together - they are independent and the homepage
        // waits on the slowest one, not the sum.
        var summaryTask = _api.GetHomeSummaryAsync(featuredCategoriesCount: 6, featuredJobsCount: 6, ct);
        var categoriesTask = _api.GetCategoriesAsync(onlyFeatured: false, page: 1, pageSize: 50, ct);
        var blogsTask = _api.GetBlogsAsync(page: 1, pageSize: 3, ct: ct);

        await Task.WhenAll(summaryTask, categoriesTask, blogsTask);

        var model = new HomeIndexViewModel
        {
            Summary = await summaryTask,
            AllCategories = (await categoriesTask).Items,
            LatestBlogs = (await blogsTask).Select(b => new BlogCardDto(b.Id, b.Title, b.Slug, b.Summary, b.ImageUrl, b.CategoryName, b.PublishedAt ?? DateTimeOffset.UtcNow, 0)).ToList()
        };

        return View(model);
    }

    public IActionResult AboutUs() => View();

    [HttpGet]
    public IActionResult ContactUs() => View(new ContactViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactUs(ContactViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.SendContactMessageAsync(new ContactRequest(
            model.Name.Trim(), model.Email.Trim(), model.Subject?.Trim(), model.Message.Trim()), ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(model);
        }

        TempData[FlashKeys.Success] = "Your message has been sent. We'll get back to you soon.";
        return RedirectToAction(nameof(ContactUs));
    }

    public IActionResult Faq() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
