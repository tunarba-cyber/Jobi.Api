namespace LinkedIn.MVC.Models.Api;

public sealed record JobStatsDto(long JobId, string Title, string Slug, JobStatus Status, int Views, int Applications, int Shortlisted, int Hired);
public sealed record DailyCountDto(DateOnly Day, int Count);
public sealed record EmployerAnalyticsDto(int TotalViews, int TotalApplications, int ActiveJobs, int Shortlisted, int Hired,
    double ConversionPercent, IReadOnlyList<JobStatsDto> Jobs, IReadOnlyList<DailyCountDto> ApplicationsLast30Days);