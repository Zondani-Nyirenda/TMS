using TMS.Domain.Common;

namespace TMS.Domain.Entities;

/// <summary>
/// Represents a category within a subject (e.g. Books, Past Papers, Video Lessons).
/// </summary>
public class MaterialCategory : BaseEntity
{
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public int SortOrder { get; set; } = 0;

    // Navigation
    public ICollection<StudyMaterial> Materials { get; set; } = new List<StudyMaterial>();
}