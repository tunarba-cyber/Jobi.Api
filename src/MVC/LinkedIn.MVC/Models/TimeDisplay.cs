namespace LinkedIn.MVC.Models;

public static class TimeDisplay
{
    private static readonly TimeZoneInfo Baku = Find();

    private static TimeZoneInfo Find()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"); }
        catch { return TimeZoneInfo.CreateCustomTimeZone("Baku", TimeSpan.FromHours(4), "Baku", "Baku"); }
    }

    public static string Local(DateTimeOffset t) => TimeZoneInfo.ConvertTime(t, Baku).ToString("dd MMM yyyy, HH:mm");
}