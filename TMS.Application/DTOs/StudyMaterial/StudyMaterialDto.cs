using TMS.Domain.Enums;

namespace TMS.Application.DTOs.StudyMaterial;

public class StudyMaterialDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MaterialType Type { get; set; }
    public string TypeLabel => Type.ToString();
    public MaterialFileType FileType { get; set; }
    public MaterialAccessLevel AccessLevel { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1048576 => $"{FileSizeBytes / 1024.0:F1} KB",
        < 1073741824 => $"{FileSizeBytes / 1048576.0:F1} MB",
        _ => $"{FileSizeBytes / 1073741824.0:F1} GB"
    };
    public string MimeType { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public string? DurationFormatted => DurationSeconds.HasValue
        ? TimeSpan.FromSeconds(DurationSeconds.Value).ToString(@"hh\:mm\:ss")
        : null;
    public bool AllowDownload { get; set; }
    public string? Tags { get; set; }
    public int? AcademicYear { get; set; }
    public string? GradeLevel { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public string? UploadedByUserId { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Student-specific (populated when student is viewing)
    public bool IsBookmarked { get; set; }
    public int? VideoProgressSeconds { get; set; }
    public double? VideoProgressPercentage { get; set; }
    public bool VideoCompleted { get; set; }
}

public class StudyMaterialSummaryDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MaterialType Type { get; set; }
    public MaterialFileType FileType { get; set; }
    public string? ThumbnailUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public int? DurationSeconds { get; set; }
    public bool AllowDownload { get; set; }
    public string? GradeLevel { get; set; }
    public int? AcademicYear { get; set; }
    public int ViewCount { get; set; }
    public bool IsBookmarked { get; set; }
    public double? VideoProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStudyMaterialRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MaterialType Type { get; set; }
    public MaterialFileType FileType { get; set; }
    public MaterialAccessLevel AccessLevel { get; set; } = MaterialAccessLevel.AllStudents;
    public bool AllowDownload { get; set; } = true;
    public string? Tags { get; set; }
    public int? AcademicYear { get; set; }
    public string? GradeLevel { get; set; }
    public int? DurationSeconds { get; set; }

    // File data — sent as base64 from Blazor WASM
    public string FileBase64 { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    // Optional thumbnail
    public string? ThumbnailBase64 { get; set; }
    public string? ThumbnailFileName { get; set; }
}

public class UpdateStudyMaterialRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MaterialAccessLevel AccessLevel { get; set; }
    public bool AllowDownload { get; set; }
    public string? Tags { get; set; }
    public int? AcademicYear { get; set; }
    public string? GradeLevel { get; set; }
    public int? DurationSeconds { get; set; }

    // Optional file replacement
    public string? FileBase64 { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? FileSizeBytes { get; set; }

    // Optional thumbnail replacement
    public string? ThumbnailBase64 { get; set; }
    public string? ThumbnailFileName { get; set; }
}

public class MaterialFilterQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? CategoryId { get; set; }
    public MaterialType? Type { get; set; }
    public string? GradeLevel { get; set; }
    public int? AcademicYear { get; set; }
    public bool? AllowDownload { get; set; }
}