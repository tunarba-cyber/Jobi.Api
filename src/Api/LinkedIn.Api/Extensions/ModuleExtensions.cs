using System.Reflection;
using LinkedIn.Shared.Abstractions.Modules;
using LinkedIn.Shared.Infrastructure.Behaviors;
using FluentValidation;

namespace LinkedIn.Api.Extensions;

/// <summary>
/// Discovers and wires every IModule. This is the piece that makes the Users
/// module's half-registered state in Pronia structurally impossible: if a module
/// is referenced, it is registered and mapped - there is no manual step to forget.
/// </summary>
public static class ModuleExtensions
{
    // The one place this prefix is defined. Every module project's assembly
    // name MUST start with this exact string, or DiscoverModules() will
    // silently find zero modules and AddMediatR will throw "No assemblies
    // found to scan" at startup - a confusing failure with no obvious cause.
    // If the solution is ever renamed again, update this constant and nothing else.
    private const string ModuleAssemblyPrefix = "LinkedIn.Modules.";

    public static IReadOnlyList<IModule> DiscoverModules(this IServiceCollection services)
    {
        // The CLR loads assemblies lazily, so a module assembly may not be in
        // AppDomain yet even though it is referenced. Force-load them first,
        // otherwise discovery silently returns nothing and the API starts with
        // zero endpoints - a very confusing failure mode.
        LoadReferencedModuleAssemblies();

        var modules = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && a.FullName?.StartsWith(ModuleAssemblyPrefix, StringComparison.Ordinal) == true)
            .SelectMany(SafeGetTypes)
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(Activator.CreateInstance)
            .Cast<IModule>()
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();

        return modules;
    }

    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration,
        IReadOnlyList<IModule> modules)
    {
        foreach (var module in modules)
            module.RegisterServices(services, configuration);

        var moduleAssemblies = modules
            .Select(m => m.GetType().Assembly)
            .Distinct()
            .ToArray();

        // Registered ONCE across all modules so pipeline behaviors are not duplicated.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(moduleAssemblies);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // runs closest to the handler
        });

        services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);

        return services;
    }

    public static WebApplication MapModuleEndpoints(this WebApplication app, IReadOnlyList<IModule> modules)
    {
        foreach (var module in modules)
            module.MapEndpoints(app);

        return app;
    }

    private static void LoadReferencedModuleAssemblies()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null) return;

        foreach (var reference in entryAssembly.GetReferencedAssemblies())
        {
            if (reference.Name?.StartsWith(ModuleAssemblyPrefix, StringComparison.Ordinal) != true)
                continue;

            try
            {
                Assembly.Load(reference);
            }
            catch (Exception ex) when (ex is FileNotFoundException or BadImageFormatException)
            {
                // A referenced module that cannot be loaded is a deployment problem,
                // not something to crash discovery over.
            }
        }
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
