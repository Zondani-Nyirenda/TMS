using TMS.Domain.Common;

namespace TMS.Domain.Entities;

/// <summary>
/// Represents a subject folder (e.g. Mathematics, English, Biology).
/// Acts as the top-level container for all study materials.
/// </summary>
public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Emoji or icon identifier e.g. "📘" or "math-icon"</summary>
    public string? IconEmoji { get; set; }

    /// <summary>Hex color for UI card e.g. "#4F46E5"</summary>
    public string? ColorHex { get; set; }

    /// <summary>Controls display order on the subjects page.</summary>
    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<MaterialCategory> Categories { get; set; } = new List<MaterialCategory>();
    public ICollection<SubjectClassMapping> ClassMappings { get; set; } = new List<SubjectClassMapping>();
}