namespace LinkedIn.MVC.Models.Api;

public enum InterviewMode { Online, OnSite }
public enum InterviewStatus { Scheduled, Confirmed, Declined, Cancelled }

public sealed record InterviewDto(long Id, long ApplicationId, string JobTitle, string CompanyName, string CandidateName,
    DateTimeOffset ScheduledAtUtc, int DurationMinutes, InterviewMode Mode, string LocationOrLink, string? Notes, InterviewStatus Status);

// Mode is an int on purpose: the API has no JsonStringEnumConverter.
public sealed record ScheduleInterviewRequest(long ApplicationId, DateTimeOffset ScheduledAtUtc, int DurationMinutes, int Mode, string LocationOrLink, string? Notes);