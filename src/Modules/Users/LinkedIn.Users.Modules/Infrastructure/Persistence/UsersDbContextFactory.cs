using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LinkedIn.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Lets "Add-Migration -Context UsersDbContext" run without booting the API
/// host, same as JobsDbContextFactory. Design-time connection only - never
/// used at runtime.
/// </summary>
public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("JOBI_DESIGNTIME_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=LinkedInDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", UsersSchema.Name))
            .Options;

        return new UsersDbContext(options);
    }
}
