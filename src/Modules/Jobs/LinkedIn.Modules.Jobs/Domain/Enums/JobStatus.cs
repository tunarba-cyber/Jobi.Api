namespace LinkedIn.Modules.Jobs.Domain.Enums;

/// <summary>
/// Draft: being written, never shown on any public page.
/// Active: live - shown on index.html, job-list-v*.html and job-details-v*.html.
/// Closed: past its deadline or filled - kept for records, hidden from public lists.
/// </summary>
public enum JobStatus
{
    Draft,
    Active,
    Closed
}
