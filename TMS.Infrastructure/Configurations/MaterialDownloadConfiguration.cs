using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class MaterialDownloadConfiguration : IEntityTypeConfiguration<MaterialDownload>
{
    public void Configure(EntityTypeBuilder<MaterialDownload> builder)
    {
        builder.ToTable("MaterialDownloads");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.IpAddress)
            .HasMaxLength(45);  // supports IPv6

        builder.Property(d => d.DownloadedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(d => d.MaterialId);
        builder.HasIndex(d => d.StudentId);
        builder.HasIndex(d => d.DownloadedAt);
        builder.HasIndex(d => d.IsDeleted);

        // Relationships
        builder.HasOne(d => d.Student)
            .WithMany(s => s.MaterialDownloads)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}