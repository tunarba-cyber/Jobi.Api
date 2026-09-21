using LinkedIn.Modules.Blogs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Blogs.Infrastructure.Persistence;

public class BlogsDbContext : DbContext
{
    public const string Schema = "blogs";

    public BlogsDbContext(DbContextOptions<BlogsDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<BlogCategory> Categories => Set<BlogCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
