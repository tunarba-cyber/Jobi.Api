using LinkedIn.Modules.Blogs.Features.Blogs;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Modules;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Blogs;

/// <summary>
/// Implements IModule so the host's DiscoverModules()/AddModules() picks this up
/// automatically - same as JobsModule/UsersModule. This replaces the old static
/// AddBlogsModule/MapBlogsModule extension methods, which compiled fine but were
/// never actually called from anywhere, so the module was dead code at runtime.
/// </summary>
public sealed class BlogsModule : IModule
{
    public string Name => "Blogs";
    public string Schema => BlogsDbContext.Schema;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BlogsDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, BlogsDbContext.Schema);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditingInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        // NOT calling AddMediatR/AddValidatorsFromAssembly here - the host's
        // AddModules() already scans every discovered module's assembly ONCE and
        // registers MediatR handlers + FluentValidation validators + the shared
        // pipeline behaviors (logging, validation) for all of them together.
        // Registering MediatR again here would just be redundant now that this
        // assembly is actually being discovered.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapBlogEndpoints();
        endpoints.MapBlogCategoryEndpoints();
    }
}
