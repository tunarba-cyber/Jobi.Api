using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence;

public sealed class MessagingDbContextFactory : IDesignTimeDbContextFactory<MessagingDbContext>
{
    public MessagingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("JOBI_DESIGNTIME_CONNECTION")
            ?? "Server=.//SQLEXPRESS;Database=LinkedIn;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", MessagingSchema.Name))
            .Options;

        return new MessagingDbContext(options);
    }
}
