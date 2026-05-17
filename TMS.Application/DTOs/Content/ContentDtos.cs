using System.ComponentModel.DataAnnotations;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Content;

// ── ContentModule ─────────────────────────────────────────────────────────────

public class ContentModuleDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ContentModuleStatus Status { get; set; }
    public int ItemCount { get; set; }   // enriched
    public DateTime CreatedAt { get; set; }
}

public class CreateContentModuleRequest
{
    [Required] public Guid CourseId { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
}

public class UpdateContentModuleRequest
{
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ContentModuleStatus Status { get; set; }
}

// ── ContentItem ───────────────────────────────────────────────────────────────

public class ContentItemDto
{
    public Guid Id { get; set; }
    public Guid ContentModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ResourceType ResourceType { get; set; }
    public ResourceAccessLevel AccessLevel { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ExternalUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsPublished { get; set; }
    public string? UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Enriched in detail calls
    public string? DownloadUrl { get; set; }
    public bool IsCompleted { get; set; }   // current student's progress
    public int? LastPositionSeconds { get; set; }

    // Computed helpers
    public string FileSizeDisplay => FileSizeBytes.HasValue
        ? FileSizeBytes.Value switch
        {
            < 1024 => $"{FileSizeBytes} B",
            < 1024 * 1024 => $"{FileSizeBytes.Value / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{FileSizeBytes.Value / (1024.0 * 1024):F1} MB",
            _ => $"{FileSizeBytes.Value / (1024.0 * 1024 * 1024):F1} GB"
        }
        : string.Empty;
}

public class ContentItemSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public ResourceAccessLevel AccessLevel { get; set; }  // add
    public string? ExternalUrl { get; set; }               // add
    public bool IsPublished { get; set; }
    public long? FileSizeBytes { get; set; }
    public int SortOrder { get; set; }
    public bool IsCompleted { get; set; }
    public int? DurationSeconds { get; set; }

    public string FileSizeDisplay => FileSizeBytes.HasValue
        ? FormatFileSize(FileSizeBytes.Value) : "-";

    private static string FormatFileSize(long bytes)
    {
        if (bytes <= 0) return "0 B";
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1) { order++; size /= 1024; }
        return $"{size:0.##} {sizes[order]}";
    }
}

public class CreateContentItemRequest
{
    [Required] public Guid ContentModuleId { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    [Required] public ResourceType ResourceType { get; set; }
    public ResourceAccessLevel AccessLevel { get; set; } = ResourceAccessLevel.AllStudents;
    // For Link type
    [MaxLength(2000)] public string? ExternalUrl { get; set; }
    public bool IsPublished { get; set; } = false;
}

public class UpdateContentItemRequest
{
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ResourceAccessLevel AccessLevel { get; set; }
    public bool IsPublished { get; set; }
    [MaxLength(2000)] public string? ExternalUrl { get; set; }
}

// ── Progress ──────────────────────────────────────────────────────────────────

public class MarkProgressRequest
{
    [Required] public Guid StudentId { get; set; }
    [Required] public Guid ContentItemId { get; set; }
    public bool IsCompleted { get; set; } = true;
    public int? LastPositionSeconds { get; set; }
}

// ── Access grant ──────────────────────────────────────────────────────────────

public class GrantAccessRequest
{
    [Required] public Guid ContentItemId { get; set; }
    [Required] public Guid StudentId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

// ── Course content overview ───────────────────────────────────────────────────

/// <summary>
/// Full content outline for a course — modules with item lists.
/// Used by the course content page.
/// </summary>
public class CourseContentDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public List<ContentModuleWithItemsDto> Modules { get; set; } = new();
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }   // for current student
    public double CompletionPct => TotalItems == 0 ? 0
        : Math.Round(CompletedItems * 100.0 / TotalItems, 1);
}

public class ContentModuleWithItemsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public ContentModuleStatus Status { get; set; }
    public List<ContentItemSummaryDto> Items { get; set; } = new();
    public int CompletedItems { get; set; }
    public double CompletionPct => Items.Count == 0 ? 0
        : Math.Round(Items.Count(i => i.IsCompleted) * 100.0 / Items.Count, 1);
}