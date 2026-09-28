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