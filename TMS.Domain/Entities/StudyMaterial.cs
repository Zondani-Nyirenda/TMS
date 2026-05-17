using TMS.Domain.Common;
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// Represents a single learning resource (book, video, past paper, etc.)
/// </summary>
public class StudyMaterial : BaseEntity
{
    public Guid CategoryId { get; set; }
    public MaterialCategory Category { get; set; } = null!;

    // Core fields
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MaterialType Type { get; set; }
    public MaterialFileType FileType { get; set; }

    // File storage
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;

    // Video-specific
    public int? DurationSeconds { get; set; }

    // Access control
    public bool AllowDownload { get; set; } = true;
    public MaterialAccessLevel AccessLevel { get; set; } = MaterialAccessLevel.AllStudents;

    // Metadata
    public string? Tags { get; set; }          // comma-separated e.g. "2024,ECZ,Paper1"
    public int? AcademicYear { get; set; }     // e.g. 2024
    public string? GradeLevel { get; set; }    // e.g. "Grade 10", "Grade 12"
    public int ViewCount { get; set; } = 0;
    public int DownloadCount { get; set; } = 0;

    // Uploaded by
    public string? UploadedByUserId { get; set; }
    public ApplicationUser? UploadedByUser { get; set; }

    // Navigation
    public ICollection<MaterialDownload> Downloads { get; set; } = new List<MaterialDownload>();
    public ICollection<MaterialBookmark> Bookmarks { get; set; } = new List<MaterialBookmark>();
    public ICollection<VideoProgress> VideoProgresses { get; set; } = new List<VideoProgress>();
}