namespace LinkedIn.MVC.Services;

public sealed class JobiApiOptions
{
    public const string SectionName = "JobiApi";

    /// <summary>e.g. "http://localhost:5292" - the API's http launch profile.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;
}
