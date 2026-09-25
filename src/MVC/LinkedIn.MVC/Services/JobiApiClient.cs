using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using LinkedIn.MVC.Models.Api;
using Microsoft.AspNetCore.WebUtilities;

namespace LinkedIn.MVC.Services;

/// <summary>
/// Typed HttpClient over LinkedIn.Api. Registered with AddHttpClient, so the
/// HttpClient lifetime/handler pooling is handled by the factory - never new
/// one up by hand.
/// </summary>
public sealed class JobiApiClient : IJobiApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<JobiApiClient> _logger;

    // The API does not register JsonStringEnumConverter, so enums arrive as
    // numbers today. JsonStringEnumConverter still accepts integer values, so
    // this reads both and keeps working if the API switches to string enums.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JobiApiClient(HttpClient http, ILogger<JobiApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Call before a request that needs a signed-in user once auth is wired:
    /// client.WithBearer(token). Left unused for now - all endpoints below are
    /// anonymous.
    /// </summary>
    public JobiApiClient WithBearer(string accessToken)
    {
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return this;
    }

    // -----------------------------------------------------------------------
    // Home
    // -----------------------------------------------------------------------
    public async Task<HomeSummaryDto> GetHomeSummaryAsync(
        int featuredCategoriesCount = 6,
        int featuredJobsCount = 6,
        CancellationToken ct = default)
    {
        var url = BuildUrl("api/home/summary", new()
        {
            ["featuredCategoriesCount"] = featuredCategoriesCount.ToString(),
            ["featuredJobsCount"] = featuredJobsCount.ToString()
        });

        // The homepage must render even if the API is down - an empty summary
        // degrades to zero counters instead of a 500 page.
        return await GetOrDefaultAsync(url, HomeSummaryDto.Empty, ct);
    }

    // -----------------------------------------------------------------------
    // Jobs
    // -----------------------------------------------------------------------
    public async Task<PagedResult<JobDto>> GetJobsAsync(JobSearchRequest request, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = NullIfBlank(request.Search),
            ["categorySlug"] = NullIfBlank(request.CategorySlug),
            // Enum query parameters bind by name on the API side.
            ["jobType"] = request.JobType?.ToString(),
            ["experienceLevel"] = request.ExperienceLevel?.ToString(),
            ["location"] = NullIfBlank(request.Location),
            ["onlyFeatured"] = request.OnlyFeatured ? "true" : null,
            ["sortBy"] = NullIfBlank(request.SortBy),
            ["descending"] = request.Descending ? "true" : null,
            ["page"] = request.Page.ToString(),
            ["pageSize"] = request.PageSize.ToString()
        };

        var url = BuildUrl("api/jobs", query);
        return await GetOrDefaultAsync(url, PagedResult<JobDto>.Empty(request.Page, request.PageSize), ct);
    }

    public Task<JobDto?> GetJobBySlugAsync(string slug, CancellationToken ct = default) =>
        GetOrNullAsync<JobDto>($"api/jobs/{Uri.EscapeDataString(slug)}", ct);

    // -----------------------------------------------------------------------
    // Categories
    // -----------------------------------------------------------------------
    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(
        bool onlyFeatured = false,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        var url = BuildUrl("api/categories", new()
        {
            ["onlyFeatured"] = onlyFeatured ? "true" : null,
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        });

        return await GetOrDefaultAsync(url, PagedResult<CategoryDto>.Empty(page, pageSize), ct);
    }

    public Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default) =>
        GetOrNullAsync<CategoryDto>($"api/categories/{Uri.EscapeDataString(slug)}", ct);

    // -----------------------------------------------------------------------
    // Blog
    // -----------------------------------------------------------------------
    public async Task<PagedResult<BlogCardDto>> GetBlogsAsync(
        int page = 1,
        int pageSize = 9,
        string? categorySlug = null,
        string? search = null,
        bool? onlyFeatured = null,
        CancellationToken ct = default)
    {
        var url = BuildUrl("api/blogs", new()
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["categorySlug"] = NullIfBlank(categorySlug),
            ["search"] = NullIfBlank(search),
            ["onlyFeatured"] = onlyFeatured?.ToString().ToLowerInvariant()
        });

        return await GetOrDefaultAsync(url, PagedResult<BlogCardDto>.Empty(page, pageSize), ct);
    }

    public Task<BlogDetailsDto?> GetBlogBySlugAsync(string slug, CancellationToken ct = default) =>
        GetOrNullAsync<BlogDetailsDto>($"api/blogs/{Uri.EscapeDataString(slug)}", ct);

    public async Task<IReadOnlyList<BlogCategoryDto>> GetBlogCategoriesAsync(CancellationToken ct = default) =>
        await GetOrDefaultAsync<IReadOnlyList<BlogCategoryDto>>(
            "api/blog-categories", Array.Empty<BlogCategoryDto>(), ct);

    // -----------------------------------------------------------------------
    // Plumbing
    // -----------------------------------------------------------------------
    private static string BuildUrl(string path, Dictionary<string, string?> query) =>
        QueryHelpers.AddQueryString(
            path,
            query.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                 .ToDictionary(kv => kv.Key, kv => kv.Value));

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>404 is a normal answer for a slug lookup, not an error.</summary>
    private async Task<T?> GetOrNullAsync<T>(string url, CancellationToken ct) where T : class
    {
        try
        {
            using var response = await _http.GetAsync(url, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "API call failed: GET {Url}", url);
            return null;
        }
    }

    /// <summary>
    /// List endpoints fall back to an empty page so one dead API call cannot
    /// take a whole page down - the view renders its "nothing found" state.
    /// </summary>
    private async Task<T> GetOrDefaultAsync<T>(string url, T fallback, CancellationToken ct)
    {
        try
        {
            using var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct) ?? fallback;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "API call failed: GET {Url}", url);
            return fallback;
        }
    }
}
