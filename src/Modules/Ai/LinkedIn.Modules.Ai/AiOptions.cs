namespace LinkedIn.Modules.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gemini-3.5-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";
    public int TimeoutSeconds { get; set; } = 60;
    public int DailyLimitPerUser { get; set; } = 30;
}