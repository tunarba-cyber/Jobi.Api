using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

/// <summary>
/// New controller - the template has no Blog views yet. Add
/// Views/Blog/Index.cshtml and Views/Blog/Details.cshtml from the Jobi
/// blog-v1.html / blog-details.html template pages.
/// </summary>
public class BlogController : Controller
{
    private readonly IJobiApiClient _api;

    public BlogController(IJobiApiClient api) => _api = api;

    // GET /Blog?categorySlug=design&search=ux&page=2
    public async Task<IActionResult> Index(
        string? categorySlug,
        string? search,
        int page = 1,
        CancellationToken ct = default)
    {
        var blogsTask = _api.GetBlogsAsync(page < 1 ? 1 : page, 9, categorySlug, search, ct: ct);
        var categoriesTask = _api.GetBlogCategoriesAsync(ct);

        await Task.WhenAll(blogsTask, categoriesTask);

        return View(new BlogListViewModel
        {
            Blogs = await blogsTask,
            Categories = await categoriesTask,
            CategorySlug = categorySlug,
            Search = search
        });
    }

    // GET /Blog/Details/my-post-slug
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var blog = await _api.GetBlogBySlugAsync(slug, ct);
        return blog is null ? NotFound() : View(blog);
    }
}
