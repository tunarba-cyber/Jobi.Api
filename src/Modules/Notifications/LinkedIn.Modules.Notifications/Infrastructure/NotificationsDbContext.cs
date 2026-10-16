using LinkedIn.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Notifications.Infrastructure;

public sealed class NotificationsDbContext : DbContext
{
    public const string Schema = "notifications";

    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema(Schema);
        b.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.Property(n => n.UserId).HasMaxLength(450).IsRequired();
            e.Property(n => n.Type).HasMaxLength(50).IsRequired();
            e.Property(n => n.Title).HasMaxLength(200).IsRequired();
            e.Property(n => n.Body).HasMaxLength(500);
            e.Property(n => n.Link).HasMaxLength(300);
            e.Property(n => n.GroupKey).HasMaxLength(100);
            e.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAtUtc });
        });
    }
}