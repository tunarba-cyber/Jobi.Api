using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(j => j.Slug)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(j => j.CompanyName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(j => j.Location)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(j => j.SalaryMin).HasColumnType("decimal(18,2)");
        builder.Property(j => j.SalaryMax).HasColumnType("decimal(18,2)");

        // Stored as strings (e.g. "FullTime") instead of ints - readable in the
        // database and safe to reorder the enum later without a data migration.
        builder.Property(j => j.JobType).HasConversion<string>().HasMaxLength(20);
        builder.Property(j => j.ExperienceLevel).HasConversion<string>().HasMaxLength(20);
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(j => j.CreatedBy).HasMaxLength(256);
        builder.Property(j => j.ModifiedBy).HasMaxLength(256);
        builder.Property(j => j.DeletedBy).HasMaxLength(256);

        builder.HasOne(j => j.Category)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // a category with jobs can't be hard-deleted out from under them

        // Enforces slug uniqueness at the database level, same pattern as Category.
        builder.HasIndex(j => j.Slug)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Covers the two heaviest public queries: public listing (status + featured)
        // and category-filtered listing.
        builder.HasIndex(j => new { j.Status, j.IsFeatured, j.CreatedAtUtc });
        builder.HasIndex(j => new { j.CategoryId, j.Status });
    }
}
