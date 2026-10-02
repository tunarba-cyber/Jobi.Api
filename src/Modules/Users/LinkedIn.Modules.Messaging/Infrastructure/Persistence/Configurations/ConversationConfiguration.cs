using LinkedIn.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("Conversations");
        b.HasKey(c => c.Id);

        // 64 (not 450) so the composite unique index stays under SQL Server's 1700-byte key limit.
        b.Property(c => c.UserAId).HasMaxLength(64).IsRequired();
        b.Property(c => c.UserBId).HasMaxLength(64).IsRequired();
        b.Property(c => c.LastMessagePreview).HasMaxLength(100);

        b.HasIndex(c => new { c.UserAId, c.UserBId }).IsUnique();
        b.HasIndex(c => c.UserBId);

        b.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
