using LinkedIn.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.UserAId).HasMaxLength(450).IsRequired();
        builder.Property(c => c.UserBId).HasMaxLength(450).IsRequired();
        builder.HasIndex(c => new { c.UserAId, c.UserBId }).IsUnique();
    }
}