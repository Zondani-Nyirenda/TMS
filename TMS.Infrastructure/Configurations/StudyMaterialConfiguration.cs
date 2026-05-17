using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class StudyMaterialConfiguration : IEntityTypeConfiguration<StudyMaterial>
{
    public void Configure(EntityTypeBuilder<StudyMaterial> builder)
    {
        builder.ToTable("StudyMaterials");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.FileUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.ThumbnailUrl)
            .HasMaxLength(1000);

        builder.Property(m => m.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Tags)
            .HasMaxLength(500);

        builder.Property(m => m.GradeLevel)
            .HasMaxLength(50);

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.FileType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.AccessLevel)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.FileSizeBytes)
            .HasDefaultValue(0);

        builder.Property(m => m.ViewCount)
            .HasDefaultValue(0);

        builder.Property(m => m.DownloadCount)
            .HasDefaultValue(0);

        builder.Property(m => m.AllowDownload)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(m => m.CategoryId);
        builder.HasIndex(m => m.Type);
        builder.HasIndex(m => m.AccessLevel);
        builder.HasIndex(m => m.GradeLevel);
        builder.HasIndex(m => m.IsDeleted);
        builder.HasIndex(m => m.UploadedByUserId);

        // Relationships
        builder.HasOne(m => m.UploadedByUser)
            .WithMany()
            .HasForeignKey(m => m.UploadedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Downloads)
            .WithOne(d => d.Material)
            .HasForeignKey(d => d.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Bookmarks)
            .WithOne(b => b.Material)
            .HasForeignKey(b => b.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.VideoProgresses)
            .WithOne(v => v.Material)
            .HasForeignKey(v => v.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}