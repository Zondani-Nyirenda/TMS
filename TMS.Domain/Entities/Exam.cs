using TMS.Domain.Common;
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// An exam or assessment administered in a class.
/// </summary>
public class Exam : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public ExamType Type { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassMark { get; set; }

    public bool IsPublished { get; set; } = false;   // Results visible to students?
    public string? Instructions { get; set; }

    // Navigation
    public ICollection<Result> Results { get; set; } = new List<Result>();
}

/// <summary>
/// A student's result for a specific exam.
/// </summary>
public class Result : BaseEntity
{
    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public decimal MarksObtained { get; set; }
    public decimal Percentage => Exam is null ? 0 : (MarksObtained / Exam.TotalMarks) * 100;
    public GradeLevel Grade { get; set; }
    public bool IsPassed { get; set; }

    public string? Remarks { get; set; }
    public string? GradedByUserId { get; set; }
    public DateTime? GradedAt { get; set; }
}
