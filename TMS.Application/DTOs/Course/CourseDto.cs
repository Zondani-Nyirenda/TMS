using System.ComponentModel.DataAnnotations;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Course;

// ── Course ───────────────────────────────────────────────────────────────────

public class CourseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? GradeLevel { get; set; }
    public int MaxCapacity { get; set; }
    public int DurationWeeks { get; set; }
    public CourseStatus Status { get; set; }
    public decimal FeeAmount { get; set; }
    public string FeeCurrency { get; set; } = "ZMW";
    public string FeeDescription { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int ActiveClassCount { get; set; }
    public int TotalEnrolledStudents { get; set; }
    public List<string> AssignedTutors { get; set; } = new();
}

public class CreateCourseRequest
{
    [Required] public string Code { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? GradeLevel { get; set; }
    [Range(1, 500)] public int MaxCapacity { get; set; } = 30;
    [Range(1, 260)] public int DurationWeeks { get; set; } = 12;
    [Range(0, 1000000)] public decimal FeeAmount { get; set; }
    public string FeeCurrency { get; set; } = "ZMW";
    public string FeeDescription { get; set; } = string.Empty;
}

public class UpdateCourseRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? GradeLevel { get; set; }
    [Range(1, 500)] public int MaxCapacity { get; set; }
    [Range(1, 260)] public int DurationWeeks { get; set; }
    [Range(0, 1000000)] public decimal FeeAmount { get; set; }
    public string FeeCurrency { get; set; } = "ZMW";
    public string FeeDescription { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public string? ThumbnailUrl { get; set; }
}

// ── Class ─────────────────────────────────────────────────────────────────────

public class ClassDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Room { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; }
    public int EnrolledCount { get; set; }
    public bool IsFull { get; set; }
    public ClassStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class CreateClassRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public Guid CourseId { get; set; }
    [Required] public Guid TutorId { get; set; }
    [Required] public DayOfWeekEnum DayOfWeek { get; set; }
    [Required] public TimeOnly StartTime { get; set; }
    [Required] public TimeOnly EndTime { get; set; }
    [Required] public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Room { get; set; }
    public string? Location { get; set; }
    [Range(1, 500)] public int MaxCapacity { get; set; } = 30;
    public string? Notes { get; set; }
}

public class UpdateClassRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public Guid TutorId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Room { get; set; }
    public string? Location { get; set; }
    [Range(1, 500)] public int MaxCapacity { get; set; }
    public ClassStatus Status { get; set; }
    public string? Notes { get; set; }
}
