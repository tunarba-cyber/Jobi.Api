using FluentValidation;
using LinkedIn.Modules.Blogs.Features.Blogs;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Blogs;

public static class BlogsModule
{
    public static IServiceCollection AddBlogsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BlogsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BlogsDbContext.Schema)));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BlogsModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(BlogsModule).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapBlogsModule(this IEndpointRouteBuilder app)
    {
        app.MapBlogEndpoints();
        app.MapBlogCategoryEndpoints();
        return app;
    }
}
