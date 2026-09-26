using LinkedIn.Modules.Jobs.Features.Applications;
using LinkedIn.Modules.Jobs.Features.Candidates;
using LinkedIn.Modules.Jobs.Features.Categories;
using LinkedIn.Modules.Jobs.Features.Companies;
using LinkedIn.Modules.Jobs.Features.Home;
using LinkedIn.Modules.Jobs.Features.Jobs;
using LinkedIn.Modules.Jobs.Features.SavedCandidates;
using LinkedIn.Modules.Jobs.Features.SavedJobs;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Modules;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Jobs;

/// <summary>
/// Self-registering module. The API host finds this by scanning assemblies, so
/// Program.cs never has to know the Jobs module exists.
/// </summary>
public sealed class JobsModule : IModule
{
    public string Name => "Jobs";
    public string Schema => JobsSchema.Name;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<JobsDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                // Each module keeps its OWN migrations history table inside its own
                // schema, so modules can be migrated independently.
                sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, JobsSchema.Name);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            });

            // Auditing and soft delete apply to every save in this module.
            options.AddInterceptors(
                sp.GetRequiredService<AuditingInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        // NOTE: MediatR and FluentValidation are registered ONCE by the host across
        // all module assemblies. Registering them per module (as Pronia did) would
        // duplicate the pipeline behaviors and run validation multiple times.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCategoryEndpoints();
        endpoints.MapJobEndpoints();
        endpoints.MapHomeEndpoints();
        endpoints.MapCompanyEndpoints();
        endpoints.MapApplicationEndpoints();
        endpoints.MapCandidateEndpoints();
        endpoints.MapSavedJobEndpoints();
        endpoints.MapSavedCandidateEndpoints();
    }
}
