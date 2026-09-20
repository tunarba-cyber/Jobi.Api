using LinkedIn.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Users.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.Property(t => t.AppUserId).IsRequired().HasMaxLength(450); // matches Identity's key length
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(256);
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(256);

        builder.Property(t => t.CreatedBy).HasMaxLength(256);
        builder.Property(t => t.ModifiedBy).HasMaxLength(256);
        builder.Property(t => t.DeletedBy).HasMaxLength(256);

        // The hot lookup on every refresh call: find an active token by its hash.
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.AppUserId);
    }
}
