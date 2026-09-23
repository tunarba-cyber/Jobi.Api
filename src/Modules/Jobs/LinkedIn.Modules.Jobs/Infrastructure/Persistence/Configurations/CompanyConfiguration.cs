using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        // Matches Identity's key length convention (see RefreshTokenConfiguration
        // in the Users module) - not a real FK, just a consistent string size.
        builder.Property(c => c.OwnerUserId).IsRequired().HasMaxLength(450);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(180);
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.WebsiteUrl).HasMaxLength(500);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);

        builder.Property(c => c.CreatedBy).HasMaxLength(256);
        builder.Property(c => c.ModifiedBy).HasMaxLength(256);
        builder.Property(c => c.DeletedBy).HasMaxLength(256);

        builder.HasIndex(c => c.Slug).IsUnique().HasFilter("[IsDeleted] = 0");

        // One company per owning user, for now - matches CreateCompanyHandler's
        // "you already have a company" check, enforced at the database level too.
        builder.HasIndex(c => c.OwnerUserId).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
