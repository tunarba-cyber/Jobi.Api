using System.ComponentModel.DataAnnotations;
using LinkedIn.MVC.Models.Api;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class EmployerDashboardViewModel
{
    public required CompanyDto Company { get; init; }
    public required PagedResult<JobDto> Jobs { get; init; }
}

public sealed class CompanyFormViewModel
{
    public bool IsEdit { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description must be 2000 characters or fewer.")]
    public string? Description { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Enter a valid website URL, including https://")]
    public string? WebsiteUrl { get; set; }
}
public sealed class ApplicantsViewModel
{
    public required JobDto Job { get; init; }
    public required PagedResult<ApplicantDto> Applicants { get; init; }
}
public sealed record SaveCandidateButtonViewModel(long CandidateProfileId, bool IsSaved, string ExtraClass = "");

public sealed class SavedCandidatesViewModel
{
    public IReadOnlyList<SavedCandidateDto> Items { get; init; } = Array.Empty<SavedCandidateDto>();
}

public sealed class JobFormViewModel
{
    public long? Id { get; set; }
    public string? Slug { get; set; }          // kept on edit so shared links don't break
    public bool IsFeatured { get; set; }       // preserved on edit, never set from this form
    public JobStatus Status { get; set; } = JobStatus.Draft;

    [Required(ErrorMessage = "Job title is required.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
    public long CategoryId { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public JobType JobType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }

    [Range(0, 1_000_000_000, ErrorMessage = "Enter a valid minimum salary.")]
    public decimal? SalaryMin { get; set; }

    [Range(0, 1_000_000_000, ErrorMessage = "Enter a valid maximum salary.")]
    public decimal? SalaryMax { get; set; }

    [Range(1, 1000, ErrorMessage = "Vacancies must be at least 1.")]
    public int VacancyCount { get; set; } = 1;

    [DataType(DataType.Date)]
    public DateTime? ApplicationDeadline { get; set; }

    /// <summary>Dropdown source only - reloaded by the controller, never posted.</summary>
    [BindNever]
    public IReadOnlyList<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();
}