using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class JobAlertConfiguration : IEntityTypeConfiguration<JobAlert>
{
    public void Configure(EntityTypeBuilder<JobAlert> builder)
    {
        builder.ToTable("JobAlerts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.CandidateUserId).IsRequired().HasMaxLength(450);
        builder.Property(a => a.Keyword).HasMaxLength(200);
        builder.Property(a => a.Location).HasMaxLength(200);

        builder.HasIndex(a => a.CandidateUserId);
    }
}