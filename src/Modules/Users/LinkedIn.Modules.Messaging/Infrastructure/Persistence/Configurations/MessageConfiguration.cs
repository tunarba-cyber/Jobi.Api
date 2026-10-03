using LinkedIn.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence.Configurations;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.SenderId).HasMaxLength(450).IsRequired();
        builder.Property(m => m.Content).HasMaxLength(4000).IsRequired();
        builder.HasOne(m => m.Conversation).WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => m.ConversationId);
    }
}