namespace LinkedIn.MVC.Services;

public sealed class JobiApiOptions
{
    public const string SectionName = "JobiApi";
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
