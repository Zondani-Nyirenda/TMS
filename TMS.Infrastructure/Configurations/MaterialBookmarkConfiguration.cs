using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class MaterialBookmarkConfiguration : IEntityTypeConfiguration<MaterialBookmark>
{
    public void Configure(EntityTypeBuilder<MaterialBookmark> builder)
    {
        builder.ToTable("MaterialBookmarks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookmarkedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Prevent duplicate bookmarks
        builder.HasIndex(b => new { b.StudentId, b.MaterialId }).IsUnique();
        builder.HasIndex(b => b.IsDeleted);

        // Relationships
        builder.HasOne(b => b.Student)
            .WithMany(s => s.MaterialBookmarks)
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}