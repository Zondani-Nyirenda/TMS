using TMS.Domain.Common;

namespace TMS.Domain.Entities;

/// <summary>
/// Allows students to bookmark/save materials for quick access.
/// </summary>
public class MaterialBookmark : BaseEntity
{
    public Guid MaterialId { get; set; }
    public StudyMaterial Material { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;
}