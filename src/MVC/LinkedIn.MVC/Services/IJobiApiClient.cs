using LinkedIn.MVC.Models.Api;

namespace LinkedIn.MVC.Services;

/// <summary>
/// Every call the MVC views need today. Read-only on purpose - the write
/// endpoints (create job, apply, etc.) all require a JWT, which lands with the
/// auth wiring.
/// </summary>
public interface IJobiApiClient
{
    // Home ------------------------------------------------------------------
    Task<HomeSummaryDto> GetHomeSummaryAsync(
        int featuredCategoriesCount = 6,
        int featuredJobsCount = 6,
        CancellationToken ct = default);

    // Jobs ------------------------------------------------------------------
    Task<PagedResult<JobDto>> GetJobsAsync(JobSearchRequest request, CancellationToken ct = default);

    Task<JobDto?> GetJobBySlugAsync(string slug, CancellationToken ct = default);

    // Categories ------------------------------------------------------------
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(
        bool onlyFeatured = false,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);
    // Auth --------------------------------------------------------------------
    Task<ApiCallResult<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiCallResult<AuthResultDto>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ApiCallResult<AuthResultDto>> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task<ApiCallResult<bool>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    // Candidate actions ---------------------------------------------------------
    Task<ApiCallResult<string>> UploadResumeAsync(Stream file, string fileName, string contentType, CancellationToken ct = default);
    Task<ApiCallResult<ApplicationDto>> ApplyAsync(ApplyRequest request, CancellationToken ct = default);
    Task<PagedResult<ApplicationDto>> GetMyApplicationsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiCallResult<bool>> WithdrawApplicationAsync(long applicationId, CancellationToken ct = default);

    Task<IReadOnlyList<SavedJobDto>> GetSavedJobsAsync(CancellationToken ct = default);
    Task<ApiCallResult<bool>> SaveJobAsync(long jobId, CancellationToken ct = default);
    Task<ApiCallResult<bool>> UnsaveJobAsync(long jobId, CancellationToken ct = default);
    // Employer ------------------------------------------------------------------
    Task<CompanyDto?> GetMyCompanyAsync(CancellationToken ct = default);
    Task<ApiCallResult<CompanyDto>> CreateCompanyAsync(CreateCompanyRequest request, CancellationToken ct = default);
    Task<ApiCallResult<CompanyDto>> UpdateMyCompanyAsync(UpdateCompanyRequest request, CancellationToken ct = default);

    Task<PagedResult<JobDto>> GetMyJobsAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<JobDto?> GetMyJobAsync(long id, CancellationToken ct = default);
    Task<ApiCallResult<JobDto>> CreateJobAsync(JobWriteRequest request, CancellationToken ct = default);
    Task<ApiCallResult<JobDto>> UpdateJobAsync(long id, JobWriteRequest request, CancellationToken ct = default);
    Task<ApiCallResult<bool>> DeleteJobAsync(long id, CancellationToken ct = default);
    Task<PagedResult<ApplicantDto>> GetApplicantsForJobAsync(long jobId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ApiCallResult<bool>> UpdateApplicationStatusAsync(long applicationId, ApplicationStatus status, CancellationToken ct = default);

    // Blog ------------------------------------------------------------------
    Task<PagedResult<BlogCardDto>> GetBlogsAsync(
        int page = 1,
        int pageSize = 9,
        string? categorySlug = null,
        string? search = null,
        bool? onlyFeatured = null,
        CancellationToken ct = default);

    Task<BlogDetailsDto?> GetBlogBySlugAsync(string slug, CancellationToken ct = default);

    Task<IReadOnlyList<BlogCategoryDto>> GetBlogCategoriesAsync(CancellationToken ct = default);
}

/// <summary>
/// The whole /api/jobs query string in one object, so the controller, the view
/// and the pagination links all agree on the same set of filters.
/// </summary>
public sealed record JobSearchRequest
{
    public string? Search { get; init; }
    public string? CategorySlug { get; init; }
    public JobType? JobType { get; init; }
    public ExperienceLevel? ExperienceLevel { get; init; }
    public string? Location { get; init; }
    public bool OnlyFeatured { get; init; }
    public string? SortBy { get; init; }
    public bool Descending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
