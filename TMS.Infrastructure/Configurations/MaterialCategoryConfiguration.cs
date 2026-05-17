using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class MaterialCategoryConfiguration : IEntityTypeConfiguration<MaterialCategory>
{
    public void Configure(EntityTypeBuilder<MaterialCategory> builder)
    {
        builder.ToTable("MaterialCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(300);

        builder.Property(c => c.IconEmoji)
            .HasMaxLength(10);

        builder.Property(c => c.SortOrder)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(c => c.SubjectId);
        builder.HasIndex(c => c.IsDeleted);
        builder.HasIndex(c => new { c.SubjectId, c.Name }).IsUnique();

        // Relationships
        builder.HasMany(c => c.Materials)
            .WithOne(m => m.Category)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}