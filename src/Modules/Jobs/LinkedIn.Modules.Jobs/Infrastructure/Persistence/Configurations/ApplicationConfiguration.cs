using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CandidateUserId).IsRequired().HasMaxLength(450);
        builder.Property(a => a.CandidateName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.CandidateEmail).IsRequired().HasMaxLength(256);
        builder.Property(a => a.ResumeUrl).HasMaxLength(500);
        builder.Property(a => a.CoverLetter).HasMaxLength(4000);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.Job)
            .WithMany()
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Restrict); // don't let a Job delete cascade into wiping application history

        // One application per candidate per job.
        builder.HasIndex(a => new { a.JobId, a.CandidateUserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(a => a.CandidateUserId);
        builder.HasIndex(a => new { a.JobId, a.Status });
    }
}