namespace TMS.Application.DTOs.StudyMaterial;

public class VideoProgressDto
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public string MaterialTitle { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ProgressSeconds { get; set; }
    public int TotalDurationSeconds { get; set; }
    public double ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastWatchedAt { get; set; }
}

public class UpdateVideoProgressRequest
{
    public Guid MaterialId { get; set; }
    public Guid StudentId { get; set; }
    public int ProgressSeconds { get; set; }
    public int TotalDurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
}