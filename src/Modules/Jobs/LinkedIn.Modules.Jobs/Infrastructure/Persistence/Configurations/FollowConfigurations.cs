using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> b)
    {
        b.ToTable("UserFollows");
        b.HasKey(f => f.Id);
        b.Property(f => f.FollowerUserId).IsRequired().HasMaxLength(450);
        b.Property(f => f.FollowedUserId).IsRequired().HasMaxLength(450);
        b.HasIndex(f => new { f.FollowerUserId, f.FollowedUserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(f => f.FollowedUserId);
    }
}

internal sealed class CompanyFollowConfiguration : IEntityTypeConfiguration<CompanyFollow>
{
    public void Configure(EntityTypeBuilder<CompanyFollow> b)
    {
        b.ToTable("CompanyFollows");
        b.HasKey(f => f.Id);
        b.Property(f => f.FollowerUserId).IsRequired().HasMaxLength(450);
        b.HasOne(f => f.Company).WithMany().HasForeignKey(f => f.CompanyId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(f => new { f.FollowerUserId, f.CompanyId }).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(f => f.CompanyId);
    }
}