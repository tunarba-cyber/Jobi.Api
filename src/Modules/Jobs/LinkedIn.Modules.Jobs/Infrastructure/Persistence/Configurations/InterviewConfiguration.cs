using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> b)
    {
        b.ToTable("Interviews");
        b.HasKey(i => i.Id);
        b.Property(i => i.LocationOrLink).IsRequired().HasMaxLength(500);
        b.Property(i => i.Notes).HasMaxLength(1000);
        b.Property(i => i.Mode).HasConversion<string>().HasMaxLength(20);
        b.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        b.HasOne(i => i.Application).WithMany().HasForeignKey(i => i.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(i => i.ApplicationId);
        b.HasIndex(i => i.ScheduledAtUtc);
    }
}