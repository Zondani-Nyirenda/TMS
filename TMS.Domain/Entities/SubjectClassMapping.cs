using TMS.Domain.Common;

namespace TMS.Domain.Entities;

/// <summary>
/// Maps a subject to one or more classes (multi-grade support).
/// Allows subject materials to be visible only to specific class groups.
/// </summary>
public class SubjectClassMapping : BaseEntity
{
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;
}