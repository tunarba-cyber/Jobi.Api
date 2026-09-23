using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Api.Extensions;

public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending migrations for every module DbContext at startup.
    /// Convenient in development; in production prefer running migrations as a
    /// separate deployment step so two app instances cannot migrate concurrently.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var jobsDb = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
        await jobsDb.Database.MigrateAsync();

        var usersDb = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await usersDb.Database.MigrateAsync();

        var blogsDb = scope.ServiceProvider.GetRequiredService<BlogsDbContext>();
        await blogsDb.Database.MigrateAsync();
    }
}
