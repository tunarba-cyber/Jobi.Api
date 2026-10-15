using LinkedIn.Modules.Ai.Features;
using LinkedIn.Modules.Ai.Infrastructure;
using LinkedIn.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LinkedIn.Modules.Ai;

public sealed class AiModule : IModule
{
    public string Name => "Ai";
    public string Schema => "ai"; // required by IModule; this module has no database

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .Validate(o => o.TimeoutSeconds is > 0 and <= 120, "Ai:TimeoutSeconds must be between 1 and 120.")
            .Validate(o => o.DailyLimitPerUser > 0, "Ai:DailyLimitPerUser must be positive.");

        services.AddSingleton<DailyQuota>();
        services.AddHttpClient<IAiClient, GeminiAiClient>((sp, http) =>
        {
            var o = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            http.BaseAddress = new Uri(o.BaseUrl.TrimEnd('/') + "/");
            http.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapAiEndpoints();
}