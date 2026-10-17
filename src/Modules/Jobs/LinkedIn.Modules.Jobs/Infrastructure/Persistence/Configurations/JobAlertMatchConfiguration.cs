using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class JobAlertMatchConfiguration : IEntityTypeConfiguration<JobAlertMatch>
{
    public void Configure(EntityTypeBuilder<JobAlertMatch> b)
    {
        b.ToTable("JobAlertMatches");
        b.HasKey(m => m.Id);
        b.HasIndex(m => new { m.AlertId, m.JobId }).IsUnique();
        b.HasIndex(m => m.JobId);
    }
}