namespace LinkedIn.MVC.Models.ViewModels;

/// <summary>One row in a breakdown. Filter is the enum name used in the admin list's query string.</summary>
public sealed record AnalyticsCount(string Label, int Count, string? Filter = null);

public sealed class AdminAnalyticsViewModel
{
    public int TotalUsers { get; init; }
    public int TotalCompanies { get; init; }
    public int TotalJobs { get; init; }
    public int TotalCategories { get; init; }
    public int TotalMessages { get; init; }
    public int UnreadMessages { get; init; }

    public IReadOnlyList<AnalyticsCount> UsersByRole { get; init; } = Array.Empty<AnalyticsCount>();
    public IReadOnlyList<AnalyticsCount> JobsByStatus { get; init; } = Array.Empty<AnalyticsCount>();
}