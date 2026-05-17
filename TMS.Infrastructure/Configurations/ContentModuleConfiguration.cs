using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Persistence.Configurations;

// ── ContentModule ─────────────────────────────────────────────────────────────

public class ContentModuleConfiguration : IEntityTypeConfiguration<ContentModule>
{
    public void Configure(EntityTypeBuilder<ContentModule> builder)
    {
        builder.ToTable("ContentModules");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Status)
               .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);

        builder.HasOne(e => e.Course)
               .WithMany()          // Course.cs doesn't yet have a ContentModules nav — additive
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CourseId);
        builder.HasIndex(e => new { e.CourseId, e.SortOrder });
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── ContentItem ───────────────────────────────────────────────────────────────

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.ToTable("ContentItems");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.ResourceType)
               .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.AccessLevel)
               .HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.StoragePath).HasMaxLength(500);
        builder.Property(e => e.OriginalFileName).HasMaxLength(300);
        builder.Property(e => e.ContentType).HasMaxLength(100);
        builder.Property(e => e.ExternalUrl).HasMaxLength(2000);
        builder.Property(e => e.UploadedByUserId).HasMaxLength(450);  // Identity PK length
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);

        builder.HasOne(e => e.ContentModule)
               .WithMany(m => m.Items)
               .HasForeignKey(e => e.ContentModuleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ContentModuleId);
        builder.HasIndex(e => new { e.ContentModuleId, e.SortOrder });
        builder.HasIndex(e => e.IsPublished);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── ContentItemAccess ─────────────────────────────────────────────────────────

public class ContentItemAccessConfiguration : IEntityTypeConfiguration<ContentItemAccess>
{
    public void Configure(EntityTypeBuilder<ContentItemAccess> builder)
    {
        builder.ToTable("ContentItemAccesses");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.GrantedByUserId).HasMaxLength(450);

        builder.HasOne(e => e.ContentItem)
               .WithMany(i => i.AccessGrants)
               .HasForeignKey(e => e.ContentItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Student)
               .WithMany()
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        // One grant per student per item
        builder.HasIndex(e => new { e.ContentItemId, e.StudentId }).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── StudentContentProgress ────────────────────────────────────────────────────

public class StudentContentProgressConfiguration
    : IEntityTypeConfiguration<StudentContentProgress>
{
    public void Configure(EntityTypeBuilder<StudentContentProgress> builder)
    {
        builder.ToTable("StudentContentProgress");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.ContentItem)
               .WithMany(i => i.Progress)
               .HasForeignKey(e => e.ContentItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Student)
               .WithMany()
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        // One progress record per student per item
        builder.HasIndex(e => new { e.ContentItemId, e.StudentId }).IsUnique();
        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.IsDeleted);
    }
}