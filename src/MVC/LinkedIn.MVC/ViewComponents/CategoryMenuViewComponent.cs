using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkedIn.MVC.ViewComponents;

public sealed class CategoryMenuViewComponent : ViewComponent
{
    private readonly IJobiApiClient _api;
    private readonly IMemoryCache _cache;
    public CategoryMenuViewComponent(IJobiApiClient api, IMemoryCache cache) { _api = api; _cache = cache; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _cache.GetOrCreateAsync("menu-categories", async entry =>
        {
            var list = (await _api.GetCategoriesAsync(onlyFeatured: false, page: 1, pageSize: 12, ct: HttpContext.RequestAborted)).Items;
            // An empty answer usually means the API was down, so retry soon instead of caching it for 5 minutes.
            entry.AbsoluteExpirationRelativeToNow = list.Count == 0 ? TimeSpan.FromSeconds(15) : TimeSpan.FromMinutes(5);
            return list;
        });

        return View(items ?? Array.Empty<CategoryDto>());
    }
}