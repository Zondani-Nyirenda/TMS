using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class VideoProgressConfiguration : IEntityTypeConfiguration<VideoProgress>
{
    public void Configure(EntityTypeBuilder<VideoProgress> builder)
    {
        builder.ToTable("VideoProgress");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProgressSeconds)
            .HasDefaultValue(0);

        builder.Property(v => v.TotalDurationSeconds)
            .HasDefaultValue(0);

        builder.Property(v => v.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(v => v.LastWatchedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Ignore computed property — not stored in DB
        builder.Ignore(v => v.ProgressPercentage);

        // One record per student per video
        builder.HasIndex(v => new { v.StudentId, v.MaterialId }).IsUnique();
        builder.HasIndex(v => v.IsDeleted);

        // Relationships
        builder.HasOne(v => v.Student)
            .WithMany(s => s.VideoProgresses)
            .HasForeignKey(v => v.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}