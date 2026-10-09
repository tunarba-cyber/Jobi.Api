using LinkedIn.Modules.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Messaging.Infrastructure.Persistence.Configurations;

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(255).IsRequired();
        builder.Property(m => m.Subject).HasMaxLength(200);
        builder.Property(m => m.Body).HasMaxLength(4000).IsRequired();
        builder.HasIndex(m => new { m.IsRead, m.CreatedAtUtc });
    }
}