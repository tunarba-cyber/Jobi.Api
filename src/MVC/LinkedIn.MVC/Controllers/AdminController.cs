using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IJobiApiClient _api;
    public AdminController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var blogsTask = _api.GetBlogsAsync(1, 10, ct);
        var categoriesTask = _api.GetBlogCategoriesAsync(ct);

        await Task.WhenAll(blogsTask, categoriesTask);

        var viewModel = new
        {
            RecentBlogs = await blogsTask,
            TotalBlogs = (await blogsTask).Count,
            Categories = await categoriesTask,
            TotalCategories = (await categoriesTask).Count
        };

        return View(viewModel);
    }

    // -------------------------------------------------------------------
    // Blogs
    // -------------------------------------------------------------------
    public async Task<IActionResult> Blogs(CancellationToken ct) =>
        View(await _api.GetBlogsAsync(ct: ct));

    public async Task<IActionResult> CreateBlog(CancellationToken ct)
    {
        ViewBag.Categories = await _api.GetBlogCategoriesAsync(ct);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBlog(CreateBlogRequest request, IFormFile? image, CancellationToken ct)
    {
        string? imageUrl = request.ImageUrl;
        if (image is { Length: > 0 })
        {
            await using var stream = image.OpenReadStream();
            var upload = await _api.UploadPhotoAsync(stream, image.FileName, image.ContentType, ct);
            if (!upload.Success)
            {
                ModelState.AddModelError(string.Empty, upload.ErrorMessage!);
                ViewBag.Categories = await _api.GetBlogCategoriesAsync(ct);
                return View(request);
            }
            imageUrl = upload.Value;
        }

        var result = await _api.CreateBlogAsync(request with { ImageUrl = imageUrl }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create blog.");
            ViewBag.Categories = await _api.GetBlogCategoriesAsync(ct);
            return View(request);
        }
        return RedirectToAction(nameof(Blogs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBlog(Guid id, CancellationToken ct)
    {
        var result = await _api.DeleteBlogAsync(id, ct);
        if (!result.Success)
            TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Blogs));
    }

    // -------------------------------------------------------------------
    // Blog Categories
    // -------------------------------------------------------------------
    public async Task<IActionResult> Categories(CancellationToken ct) =>
        View(await _api.GetBlogCategoriesAsync(ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, CancellationToken ct)
    {
        var result = await _api.CreateBlogCategoryAsync(name, ct);
        if (!result.Success)
            TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(Categories));
    }

    // -------------------------------------------------------------------
    // Job Categories (separate from Blog Categories above - different
    // entity, different API, different purpose: these power job filtering)
    // -------------------------------------------------------------------
    public async Task<IActionResult> JobCategories(CancellationToken ct) =>
        View(await _api.GetCategoriesAsync(includeInactive: true, pageSize: 100, ct: ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJobCategory(CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _api.CreateCategoryAsync(request, ct);
        if (!result.Success)
            TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(JobCategories));
    }
    public async Task<IActionResult> EditJobCategory(long id, CancellationToken ct)
    {
        var categories = await _api.GetCategoriesAsync(includeInactive: true, pageSize: 100, ct: ct);
        var category = categories.Items.FirstOrDefault(c => c.Id == id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJobCategory(long id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await _api.UpdateCategoryAsync(id, request, ct);
        if (!result.Success) TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(JobCategories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJobCategory(long id, CancellationToken ct)
    {
        var result = await _api.DeleteCategoryAsync(id, ct);
        if (!result.Success)
            TempData["Error"] = result.ErrorMessage;
        return RedirectToAction(nameof(JobCategories));
    }
}