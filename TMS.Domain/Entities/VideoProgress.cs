using TMS.Domain.Common;

namespace TMS.Domain.Entities;

/// <summary>
/// Tracks a student's video watch progress for "continue watching" feature.
/// One record per student per video — upserted on each progress update.
/// </summary>
public class VideoProgress : BaseEntity
{
    public Guid MaterialId { get; set; }
    public StudyMaterial Material { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    /// <summary>How far the student has watched in seconds.</summary>
    public int ProgressSeconds { get; set; } = 0;

    /// <summary>Total duration in seconds (denormalised for quick % calc).</summary>
    public int TotalDurationSeconds { get; set; } = 0;

    /// <summary>Percentage watched — computed, not stored.</summary>
    public double ProgressPercentage =>
        TotalDurationSeconds == 0 ? 0 :
        Math.Round(ProgressSeconds * 100.0 / TotalDurationSeconds, 1);

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
}