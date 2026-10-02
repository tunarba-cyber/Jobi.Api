using LinkedIn.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence.Configurations;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("Messages");
        b.HasKey(m => m.Id);

        b.Property(m => m.SenderId).HasMaxLength(64).IsRequired();
        b.Property(m => m.Body).HasMaxLength(4000).IsRequired();

        // Paging a conversation newest-first.
        b.HasIndex(m => new { m.ConversationId, m.SentAtUtc });
    }
}
