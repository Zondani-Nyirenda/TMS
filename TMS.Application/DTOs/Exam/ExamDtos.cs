using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Exam;

public class ExamDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public ExamType Type { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassMark { get; set; }
    public bool IsPublished { get; set; }
    public string? Instructions { get; set; }
    public int ResultCount { get; set; }
    public double? AverageScore { get; set; }
    public double? PassRate { get; set; }
}

public class CreateExamRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public Guid ClassId { get; set; }
    [Required] public ExamType Type { get; set; }
    [Required] public DateTime ExamDate { get; set; }
    [Range(1, 1000)] public decimal TotalMarks { get; set; } = 100;
    [Range(0, 1000)] public decimal PassMark { get; set; } = 50;
    public string? Instructions { get; set; }
}

public class UpdateExamRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExamType Type { get; set; }
    public DateTime ExamDate { get; set; }
    [Range(1, 1000)] public decimal TotalMarks { get; set; }
    [Range(0, 1000)] public decimal PassMark { get; set; }
    public string? Instructions { get; set; }
    public bool IsPublished { get; set; }
}

public class ResultDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal Percentage { get; set; }
    public GradeLevel Grade { get; set; }
    public bool IsPassed { get; set; }
    public string? Remarks { get; set; }
    public DateTime? GradedAt { get; set; }
}

/// <summary>
/// Bulk-enter results for all students in a class.
/// </summary>
public class SubmitResultsRequest
{
    [Required] public Guid ExamId { get; set; }
    [Required, MinLength(1)] public List<StudentResultEntry> Entries { get; set; } = new();
}

public class StudentResultEntry
{
    [Required] public Guid StudentId { get; set; }
    [Range(0, 10000)] public decimal MarksObtained { get; set; }
    public string? Remarks { get; set; }
}

public class StudentPerformanceDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<ExamResultSummary> ExamResults { get; set; } = new();
    public double OverallAverage { get; set; }
    public double PassRate { get; set; }
    public GradeLevel OverallGrade { get; set; }
}

public class ExamResultSummary
{
    public string ExamTitle { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal Percentage { get; set; }
    public GradeLevel Grade { get; set; }
    public bool IsPassed { get; set; }
}
