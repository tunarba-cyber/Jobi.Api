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
    public async Task<ApiCallResult<string>> UploadResumeAsync(Stream file, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(file);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName); // "file" must match the API's IFormFile parameter name

        using var response = await _http.PostAsync("api/uploads/resume", content, ct);
        if (!response.IsSuccessStatusCode)
            return ApiCallResult<string>.Fail(await ReadErrorAsync(response, ct));

        var body = await response.Content.ReadFromJsonAsync<UploadResultDto>(JsonOptions, ct);
        return body?.Url is { } url
            ? ApiCallResult<string>.Ok(url)
            : ApiCallResult<string>.Fail("Upload succeeded but no file URL was returned.");
    }

    public async Task<ApiCallResult<ApplicationDto>> ApplyAsync(ApplyRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/applications", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
            return ApiCallResult<ApplicationDto>.Fail(await ReadErrorAsync(response, ct));

        var dto = await response.Content.ReadFromJsonAsync<ApplicationDto>(JsonOptions, ct);
        return dto is not null
            ? ApiCallResult<ApplicationDto>.Ok(dto)
            : ApiCallResult<ApplicationDto>.Fail("Unexpected empty response.");
    }

    public async Task<PagedResult<ApplicationDto>> GetMyApplicationsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var url = BuildUrl("api/applications/mine", new()
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        });
        return await GetOrDefaultAsync(url, PagedResult<ApplicationDto>.Empty(page, pageSize), ct);
    }

    public async Task<ApiCallResult<bool>> WithdrawApplicationAsync(long applicationId, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/applications/{applicationId}", ct);
        return response.IsSuccessStatusCode
            ? ApiCallResult<bool>.Ok(true)
            : ApiCallResult<bool>.Fail(await ReadErrorAsync(response, ct));
    }

    public async Task<IReadOnlyList<SavedJobDto>> GetSavedJobsAsync(CancellationToken ct = default) =>
        await GetOrDefaultAsync<IReadOnlyList<SavedJobDto>>("api/saved-jobs", Array.Empty<SavedJobDto>(), ct);

    public async Task<ApiCallResult<bool>> SaveJobAsync(long jobId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/saved-jobs/{jobId}", null, ct);
        // 409 means "already saved" - the end state the user wanted, so not an error.
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict)
            return ApiCallResult<bool>.Ok(true);
        return ApiCallResult<bool>.Fail(await ReadErrorAsync(response, ct));
    }

    public async Task<ApiCallResult<bool>> UnsaveJobAsync(long jobId, CancellationToken ct = default)
    {
        using var response = await _http.DeleteAsync($"api/saved-jobs/{jobId}", ct);
        return response.IsSuccessStatusCode
            ? ApiCallResult<bool>.Ok(true)
            : ApiCallResult<bool>.Fail(await ReadErrorAsync(response, ct));
    }
    public Task<CompanyDto?> GetMyCompanyAsync(CancellationToken ct = default) =>
    GetOrNullAsync<CompanyDto>("api/companies/me", ct);

    public Task<ApiCallResult<CompanyDto>> CreateCompanyAsync(CreateCompanyRequest request, CancellationToken ct = default) =>
        SendJsonAsync<CompanyDto>(HttpMethod.Post, "api/companies", request, ct);

    public Task<ApiCallResult<CompanyDto>> UpdateMyCompanyAsync(UpdateCompanyRequest request, CancellationToken ct = default) =>
        SendJsonAsync<CompanyDto>(HttpMethod.Put, "api/companies/me", request, ct);

    public async Task<PagedResult<JobDto>> GetMyJobsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var url = BuildUrl("api/jobs/mine", new()
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        });
        return await GetOrDefaultAsync(url, PagedResult<JobDto>.Empty(page, pageSize), ct);
    }

    public Task<JobDto?> GetMyJobAsync(long id, CancellationToken ct = default) =>
        GetOrNullAsync<JobDto>($"api/jobs/mine/{id}", ct);

    public Task<ApiCallResult<JobDto>> CreateJobAsync(JobWriteRequest request, CancellationToken ct = default) =>
        SendJsonAsync<JobDto>(HttpMethod.Post, "api/jobs", request, ct);

    public Task<ApiCallResult<JobDto>> UpdateJobAsync(long id, JobWriteRequest request, CancellationToken ct = default) =>
        SendJsonAsync<JobDto>(HttpMethod.Put, $"api/jobs/{id}", request, ct);

    public async Task<ApiCallResult<bool>> DeleteJobAsync(long id, CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.DeleteAsync($"api/jobs/{id}", ct);
            return response.IsSuccessStatusCode
                ? ApiCallResult<bool>.Ok(true)
                : ApiCallResult<bool>.Fail(await ReadErrorAsync(response, ct));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "API call failed: DELETE api/jobs/{Id}", id);
            return ApiCallResult<bool>.Fail("Can't reach the server. Please try again.");
        }
    }

    /// <summary>One place for "send a JSON body, get a typed result or a readable error".</summary>
    private async Task<ApiCallResult<T>> SendJsonAsync<T>(HttpMethod method, string url, object body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };
            using var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return ApiCallResult<T>.Fail(await ReadErrorAsync(response, ct));

            var dto = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
            return dto is not null
                ? ApiCallResult<T>.Ok(dto)
                : ApiCallResult<T>.Fail("Unexpected empty response.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "API call failed: {Method} {Url}", method, url);
            return ApiCallResult<T>.Fail("Can't reach the server. Please try again.");
        }
    }

    private sealed record UploadResultDto(string? Url);

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
    public async Task<ApiCallResult<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/register", request, JsonOptions, ct);
        if (response.IsSuccessStatusCode) return ApiCallResult<bool>.Ok(true);
        return ApiCallResult<bool>.Fail(await ReadErrorAsync(response, ct));
    }

    public async Task<ApiCallResult<AuthResultDto>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/login", request, JsonOptions, ct);
        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions, ct);
            return dto is not null ? ApiCallResult<AuthResultDto>.Ok(dto) : ApiCallResult<AuthResultDto>.Fail("Unexpected empty response.");
        }
        return ApiCallResult<AuthResultDto>.Fail(await ReadErrorAsync(response, ct));
    }

    public async Task<ApiCallResult<AuthResultDto>> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken }, JsonOptions, ct);
        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions, ct);
            return dto is not null ? ApiCallResult<AuthResultDto>.Ok(dto) : ApiCallResult<AuthResultDto>.Fail("Unexpected empty response.");
        }
        return ApiCallResult<AuthResultDto>.Fail(await ReadErrorAsync(response, ct));
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        try { await _http.PostAsJsonAsync("api/auth/logout", new { refreshToken }, JsonOptions, ct); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Logout failing server-side isn't worth blocking the user's sign-out for.
            _logger.LogWarning(ex, "Logout call to API failed - proceeding with local sign-out anyway.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized) return "Please log in to continue.";
        if (response.StatusCode == HttpStatusCode.Forbidden) return "Your account type can't do this.";

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(JsonOptions, ct);

            if (problem?.Errors is { Count: > 0 })
                return string.Join(" ", problem.Errors.Values.SelectMany(messages => messages));

            return problem?.Detail ?? problem?.Title ?? "Something went wrong. Please try again.";
        }
        catch { return "Something went wrong. Please try again."; }
    }
    public async Task<ApiCallResult<bool>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/confirm-email", request, JsonOptions, ct);
        if (response.IsSuccessStatusCode) return ApiCallResult<bool>.Ok(true);
        return ApiCallResult<bool>.Fail(await ReadErrorAsync(response, ct));
    }

    public async Task<PagedResult<ApplicantDto>> GetApplicantsForJobAsync(long jobId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var url = BuildUrl($"api/applications/job/{jobId}", new()
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        });
        return await GetOrDefaultAsync(url, PagedResult<ApplicantDto>.Empty(page, pageSize), ct);
    }

    public async Task<ApiCallResult<bool>> UpdateApplicationStatusAsync(long applicationId, ApplicationStatus status, CancellationToken ct = default)
    {
        // NewStatus travels as an int for the same reason JobWriteRequest's enums do -
        // the API has no JsonStringEnumConverter registered.
        var result = await SendJsonAsync<ApplicantDto>(
            HttpMethod.Put, $"api/applications/{applicationId}/status", new { newStatus = (int)status }, ct);

        return result.Success ? ApiCallResult<bool>.Ok(true) : ApiCallResult<bool>.Fail(result.ErrorMessage!);
    }
    private sealed record ProblemDetailsResponse(string? Title, string? Detail, Dictionary<string, string[]>? Errors);

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
