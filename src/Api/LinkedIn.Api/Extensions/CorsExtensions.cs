namespace LinkedIn.Api.Extensions;

/// <summary>
/// The LinkedIn template is static HTML served from its own origin (Live Server,
/// IIS, Netlify...). Without CORS, every fetch() from index.html is blocked by
/// the browser - so this is required before the frontend can talk to the API.
/// </summary>
public static class CorsExtensions
{
    public const string FrontendPolicy = "LinkedInFrontend";

    public static IServiceCollection AddLinkedInCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // Development fallback only.
                    policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }
}
