using LinkedIn.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence;

public sealed class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(MessagingSchema.Name);
        builder.ApplyConfigurationsFromAssembly(typeof(MessagingDbContext).Assembly);
    }
}
