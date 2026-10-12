using System.ComponentModel.DataAnnotations;
using LinkedIn.MVC.Models.Api;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class ScheduleInterviewViewModel
{
    public long ApplicationId { get; set; }
    public string? CandidateName { get; set; }   // display only
    public string? JobTitle { get; set; }        // display only

    [Required(ErrorMessage = "Pick a date and time.")]
    public DateTimeOffset? ScheduledAtUtc { get; set; }

    [Range(15, 480, ErrorMessage = "Duration must be 15-480 minutes.")]
    public int DurationMinutes { get; set; } = 30;

    public InterviewMode Mode { get; set; }

    [Required(ErrorMessage = "Add a location or meeting link."), MaxLength(500)]
    public string LocationOrLink { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}