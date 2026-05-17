using TMS.Domain.Common;

namespace TMS.Domain.Entities;

/// <summary>
/// Audit log of every file download by a student.
/// </summary>
public class MaterialDownload : BaseEntity
{
    public Guid MaterialId { get; set; }
    public StudyMaterial Material { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}