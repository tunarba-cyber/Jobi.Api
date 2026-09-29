using System.ComponentModel.DataAnnotations;
using LinkedIn.MVC.Models.Api;
using Microsoft.AspNetCore.Http;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class ApplyViewModel
{
    public long JobId { get; set; }
    public string JobSlug { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Cover letter must be 4000 characters or fewer.")]
    public string? CoverLetter { get; set; }

    public IFormFile? Resume { get; set; }
}

public sealed class MyApplicationsViewModel
{
    public required PagedResult<ApplicationDto> Applications { get; init; }
}

public sealed class SavedJobsViewModel
{
    public IReadOnlyList<SavedJobDto> Items { get; init; } = Array.Empty<SavedJobDto>();
}

public sealed record SaveJobButtonViewModel(long JobId, bool IsSaved, string ExtraClass = "");
public sealed class CandidateProfileViewModel
{
    public bool HasProfile { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Headline is required.")]
    [MaxLength(200)]
    public string Headline { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Bio must be 2000 characters or fewer.")]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(1000, ErrorMessage = "Skills must be 1000 characters or fewer.")]
    public string? Skills { get; set; }

    public ExperienceLevel ExperienceLevel { get; set; }
    public bool IsAvailableForWork { get; set; } = true;

    // Existing uploaded files, shown as previews - not posted back directly.
    public string? CurrentPhotoUrl { get; set; }
    public string? CurrentResumeUrl { get; set; }

    // New uploads, both optional - a saved profile keeps its old file if these are empty.
    public IFormFile? Photo { get; set; }
    public IFormFile? Resume { get; set; }
}
public sealed class CandidatesListViewModel
{
    public IReadOnlySet<long> SavedCandidateIds { get; init; } = new HashSet<long>();
    public required PagedResult<CandidateProfileDto> Candidates { get; init; }
    public required CandidateSearchRequest Filters { get; init; }

    public bool IsEmpty => Candidates.Items.Count == 0;

    public IDictionary<string, string?> RouteValuesFor(int page) => new Dictionary<string, string?>
    {
        ["search"] = Filters.Search,
        ["location"] = Filters.Location,
        ["experienceLevel"] = Filters.ExperienceLevel?.ToString(),
        ["page"] = page.ToString()
    };
}

public sealed class CompanyDetailsViewModel
{
    public required CompanyDto Company { get; init; }
}