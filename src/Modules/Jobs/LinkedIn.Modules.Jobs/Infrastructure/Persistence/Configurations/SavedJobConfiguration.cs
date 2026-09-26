using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
{
    public void Configure(EntityTypeBuilder<SavedJob> builder)
    {
        builder.ToTable("SavedJobs");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CandidateUserId).IsRequired().HasMaxLength(450);

        builder.HasOne(s => s.Job).WithMany().HasForeignKey(s => s.JobId).OnDelete(DeleteBehavior.Cascade);

        // A candidate can save a given job only once.
        builder.HasIndex(s => new { s.JobId, s.CandidateUserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => s.CandidateUserId);
    }
}