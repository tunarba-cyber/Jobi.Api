using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence;

/// <summary>
/// Lets you run "dotnet ef migrations add" from inside the module project without
/// booting the whole API host. The connection string here is design-time only and
/// is never used at runtime.
/// </summary>
public sealed class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
{
    public JobsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("JOBI_DESIGNTIME_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=LinkedInDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<JobsDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", JobsSchema.Name))
            .Options;

        return new JobsDbContext(options);
    }
}
