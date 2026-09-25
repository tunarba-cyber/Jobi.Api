using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public class JobController : Controller
{
    private readonly IJobiApiClient _api;

    private const int ListPageSize = 10;
    private const int GridPageSize = 12;

    public JobController(IJobiApiClient api) => _api = api;

    // GET /Job/JobList?search=&categorySlug=&jobType=Remote&page=2
    public Task<IActionResult> JobList(
        string? search,
        string? categorySlug,
        JobType? jobType,
        ExperienceLevel? experienceLevel,
        string? location,
        string? sortBy,
        bool descending = false,
        int page = 1,
        CancellationToken ct = default) =>
        RenderListAsync("JobList", ListPageSize, search, categorySlug, jobType,
            experienceLevel, location, sortBy, descending, page, ct);

    // Same data, different layout - the grid view just renders cards.
    public Task<IActionResult> JobGrid(
        string? search,
        string? categorySlug,
        JobType? jobType,
        ExperienceLevel? experienceLevel,
        string? location,
        string? sortBy,
        bool descending = false,
        int page = 1,
        CancellationToken ct = default) =>
        RenderListAsync("JobGrid", GridPageSize, search, categorySlug, jobType,
            experienceLevel, location, sortBy, descending, page, ct);

    // GET /Job/JobDetails/senior-ui-designer
    public async Task<IActionResult> JobDetails(string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(JobList));
        }

        var job = await _api.GetJobBySlugAsync(slug, ct);
        if (job is null)
        {
            return NotFound();
        }

        // JobDto carries CategoryName but not CategorySlug, and /api/jobs
        // filters by slug - so related jobs are matched on the category name
        // through the free-text search instead. Add CategorySlug to JobDto on
        // the API side and this becomes an exact filter.
        var related = await _api.GetJobsAsync(new JobSearchRequest
        {
            Search = job.CategoryName,
            Page = 1,
            PageSize = 5
        }, ct);

        var model = new JobDetailsViewModel
        {
            Job = job,
            RelatedJobs = related.Items.Where(j => j.Id != job.Id).Take(4).ToList()
        };

        return View(model);
    }

    private async Task<IActionResult> RenderListAsync(
        string viewName,
        int pageSize,
        string? search,
        string? categorySlug,
        JobType? jobType,
        ExperienceLevel? experienceLevel,
        string? location,
        string? sortBy,
        bool descending,
        int page,
        CancellationToken ct)
    {
        var filters = new JobSearchRequest
        {
            Search = search,
            CategorySlug = categorySlug,
            JobType = jobType,
            ExperienceLevel = experienceLevel,
            Location = location,
            SortBy = sortBy,
            Descending = descending,
            Page = page < 1 ? 1 : page,
            PageSize = pageSize
        };

        var jobsTask = _api.GetJobsAsync(filters, ct);
        var categoriesTask = _api.GetCategoriesAsync(onlyFeatured: false, page: 1, pageSize: 50, ct);

        await Task.WhenAll(jobsTask, categoriesTask);

        var model = new JobListViewModel
        {
            Jobs = await jobsTask,
            Filters = filters,
            Categories = (await categoriesTask).Items
        };

        return View(viewName, model);
    }
}
