using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Shared.Abstractions.Modules;

/// <summary>
/// Every feature module implements this. The API host discovers implementations by
/// scanning referenced assemblies, so adding a module means adding a project
/// reference - not editing Program.cs by hand (which is how Pronia did it, and how
/// the Users module ended up half-registered).
/// </summary>
public interface IModule
{
    /// <summary>Human-readable module name, used for logging and health checks.</summary>
    string Name { get; }

    /// <summary>Database schema this module owns. Keeps modules isolated inside one database.</summary>
    string Schema { get; }

    /// <summary>Register the module's own services (DbContext, module services).</summary>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Map the module's HTTP endpoints.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
