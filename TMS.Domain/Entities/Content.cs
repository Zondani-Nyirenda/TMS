using TMS.Domain.Common;
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// A top-level module (unit/week/chapter) inside a Course.
/// e.g. "Week 1 – Introduction to Calculus"
/// </summary>
public class ContentModule : BaseEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }   // display order within the course
    public ContentModuleStatus Status { get; set; } = ContentModuleStatus.Draft;

    // Navigation
    public ICollection<ContentItem> Items { get; set; } = new List<ContentItem>();
}

/// <summary>
/// A single learning resource inside a ContentModule.
/// e.g. "Lecture slides Week 1", "Tutorial video 3"
/// </summary>
public class ContentItem : BaseEntity
{
    public Guid ContentModuleId { get; set; }
    public ContentModule ContentModule { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    public ResourceType ResourceType { get; set; }
    public ResourceAccessLevel AccessLevel { get; set; } = ResourceAccessLevel.AllStudents;

    // For file-based resources (Document, Video, Audio, Image, Archive)
    public string? StoragePath { get; set; }   // relative path inside storage root
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }   // MIME type
    public long? FileSizeBytes { get; set; }

    // For Link resources
    public string? ExternalUrl { get; set; }

    // Duration in seconds (video/audio)
    public int? DurationSeconds { get; set; }

    public bool IsPublished { get; set; } = false;

    // Who uploaded this item (FK to ApplicationUser.Id string PK)
    public string? UploadedByUserId { get; set; }

    // Navigation
    public ICollection<ContentItemAccess> AccessGrants { get; set; } = new List<ContentItemAccess>();
    public ICollection<StudentContentProgress> Progress { get; set; } = new List<StudentContentProgress>();
}

/// <summary>
/// Explicit access grant for AccessLevel = Restricted resources.
/// </summary>
public class ContentItemAccess : BaseEntity
{
    public Guid ContentItemId { get; set; }
    public ContentItem ContentItem { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedByUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }   // null = no expiry
}

/// <summary>
/// Tracks which content items a student has viewed/completed.
/// Used for progress indicators and analytics.
/// </summary>
public class StudentContentProgress : BaseEntity
{
    public Guid ContentItemId { get; set; }
    public ContentItem ContentItem { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;
    public DateTime FirstAccessedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int AccessCount { get; set; } = 1;
    /// <summary>Last watched position in seconds (video/audio).</summary>
    public int? LastPositionSeconds { get; set; }
}