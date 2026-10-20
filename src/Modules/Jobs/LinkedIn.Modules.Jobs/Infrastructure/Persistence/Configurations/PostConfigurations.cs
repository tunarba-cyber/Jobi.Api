using LinkedIn.Modules.Jobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Configurations;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> b)
    {
        b.ToTable("Posts");
        b.HasKey(p => p.Id);
        b.Property(p => p.AuthorUserId).IsRequired().HasMaxLength(450);
        b.Property(p => p.Caption).IsRequired().HasMaxLength(2000);
        b.Property(p => p.ImageUrl).HasMaxLength(300);
        b.HasIndex(p => new { p.AuthorUserId, p.CreatedAtUtc });
        b.HasIndex(p => p.CreatedAtUtc);
    }
}

internal sealed class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> b)
    {
        b.ToTable("PostLikes");
        b.HasKey(l => l.Id);
        b.Property(l => l.UserId).IsRequired().HasMaxLength(450);
        b.HasOne<Post>().WithMany().HasForeignKey(l => l.PostId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(l => new { l.PostId, l.UserId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

internal sealed class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
{
    public void Configure(EntityTypeBuilder<PostComment> b)
    {
        b.ToTable("PostComments");
        b.HasKey(c => c.Id);
        b.Property(c => c.UserId).IsRequired().HasMaxLength(450);
        b.Property(c => c.Text).IsRequired().HasMaxLength(500);
        b.HasOne<Post>().WithMany().HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(c => c.PostId);
    }
}