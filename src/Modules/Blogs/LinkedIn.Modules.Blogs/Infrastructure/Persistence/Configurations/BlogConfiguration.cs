using LinkedIn.Modules.Blogs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkedIn.Modules.Blogs.Infrastructure.Persistence.Configurations;

internal sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Slug).HasMaxLength(220).IsRequired();
        builder.Property(b => b.Summary).HasMaxLength(500).IsRequired();
        builder.Property(b => b.Content).IsRequired();
        builder.Property(b => b.ImageUrl).HasMaxLength(500);

        builder.HasIndex(b => b.Slug).IsUnique();
        builder.HasIndex(b => new { b.IsPublished, b.PublishedAt });
        builder.HasIndex(b => b.IsFeatured);

        builder.HasOne(b => b.Category)
               .WithMany(c => c.Blogs)
               .HasForeignKey(b => b.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

internal sealed class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable("BlogCategories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();
    }
}
