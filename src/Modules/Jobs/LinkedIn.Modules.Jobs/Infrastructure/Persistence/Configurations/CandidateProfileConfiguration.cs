using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.ToTable("CandidateProfiles");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.OwnerUserId).IsRequired().HasMaxLength(450);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(220);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Headline).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Bio).HasMaxLength(2000);
        builder.Property(c => c.Location).HasMaxLength(200);
        builder.Property(c => c.PhotoUrl).HasMaxLength(500);
        builder.Property(c => c.ResumeUrl).HasMaxLength(500);
        builder.Property(c => c.Skills).HasMaxLength(1000);

        builder.HasIndex(c => c.Slug).IsUnique().HasFilter("[IsDeleted] = 0");

        // One profile per user - same rule as Company.
        builder.HasIndex(c => c.OwnerUserId).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => c.IsAvailableForWork);
    }
}