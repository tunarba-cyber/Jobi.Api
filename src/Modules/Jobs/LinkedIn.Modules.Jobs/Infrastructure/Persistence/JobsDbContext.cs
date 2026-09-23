using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence;

public sealed class JobsDbContext : DbContext
{
    public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(JobsSchema.Name);

        // Picks up every IEntityTypeConfiguration in this assembly automatically -
        // a new entity config is never forgotten.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsDbContext).Assembly);

        // Adds "WHERE IsDeleted = 0" to every soft-deletable entity in one sweep.
        modelBuilder.ApplySoftDeleteQueryFilter();
    }
}
