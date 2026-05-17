using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;

namespace TMS.Infrastructure.Configurations;

public class SubjectClassMappingConfiguration : IEntityTypeConfiguration<SubjectClassMapping>
{
    public void Configure(EntityTypeBuilder<SubjectClassMapping> builder)
    {
        builder.ToTable("SubjectClassMappings");

        builder.HasKey(m => m.Id);

        // Prevent duplicate subject-class pairs
        builder.HasIndex(m => new { m.SubjectId, m.ClassId }).IsUnique();
        builder.HasIndex(m => m.IsDeleted);

        // Relationships
        builder.HasOne(m => m.Subject)
            .WithMany(s => s.ClassMappings)
            .HasForeignKey(m => m.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Class)
            .WithMany(c => c.SubjectMappings)
            .HasForeignKey(m => m.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}