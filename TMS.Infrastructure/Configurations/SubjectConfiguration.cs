using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.IconEmoji)
            .HasMaxLength(10);

        builder.Property(s => s.ColorHex)
            .HasMaxLength(7);  // e.g. #4F46E5

        builder.Property(s => s.SortOrder)
            .HasDefaultValue(0);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(s => s.Name).IsUnique();
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.IsDeleted);

        // Relationships
        builder.HasMany(s => s.Categories)
            .WithOne(c => c.Subject)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.ClassMappings)
            .WithOne(m => m.Subject)
            .HasForeignKey(m => m.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}