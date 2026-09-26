using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class SavedCandidateConfiguration : IEntityTypeConfiguration<SavedCandidate>
{
    public void Configure(EntityTypeBuilder<SavedCandidate> builder)
    {
        builder.ToTable("SavedCandidates");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.EmployerUserId).IsRequired().HasMaxLength(450);

        builder.HasOne(s => s.CandidateProfile).WithMany().HasForeignKey(s => s.CandidateProfileId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.CandidateProfileId, s.EmployerUserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => s.EmployerUserId);
    }
}