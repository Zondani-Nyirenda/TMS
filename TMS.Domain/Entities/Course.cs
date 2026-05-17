using TMS.Domain.Common;
using TMS.Domain.Enums;
using TMS.Domain.ValueObjects;

namespace TMS.Domain.Entities;

/// <summary>
/// A course is a subject offered by the tuition centre (e.g. Grade 12 Mathematics).
/// </summary>
public class Course : BaseEntity
{
    public string Code { get; set; } = string.Empty;      // e.g. MATH-G12
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? GradeLevel { get; set; }               // e.g. Grade 12, Form 5
    public int MaxCapacity { get; set; } = 30;
    public int DurationWeeks { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Active;
    public string? ThumbnailUrl { get; set; }

    // Fee structure — stored as decimal then wrapped by application layer
    public decimal FeeAmount { get; set; }
    public string FeeCurrency { get; set; } = "ZMW";
    public string FeeDescription { get; set; } = string.Empty; // e.g. "Monthly tuition"

    // Navigation
    public ICollection<TutorCourse> TutorCourses { get; set; } = new List<TutorCourse>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public ICollection<FeeStructure> FeeStructures { get; set; } = new List<FeeStructure>();
}

/// <summary>
/// A class is a scheduled session of a course (time, room, tutor).
/// </summary>
public class Class : BaseEntity
{
    public string Name { get; set; } = string.Empty;      // e.g. "Mon/Wed Morning Group"
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid TutorId { get; set; }
    public Tutor Tutor { get; set; } = null!;

    // Schedule
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Venue
    public string? Room { get; set; }
    public string? Location { get; set; }

    public int MaxCapacity { get; set; } = 30;
    public ClassStatus Status { get; set; } = ClassStatus.Scheduled;
    public string? Notes { get; set; }

    // Navigation
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();

    // Computed
    public int EnrolledCount => StudentClasses.Count(sc => sc.Status == EnrollmentStatus.Active);
    public bool IsFull => EnrolledCount >= MaxCapacity;

    // Inside Class : BaseEntity — add to navigation collections
    public ICollection<SubjectClassMapping> SubjectMappings { get; set; } = new List<SubjectClassMapping>();
}

/// <summary>
/// Join table: many-to-many between Student and Class with extra enrollment data.
/// </summary>
public class StudentClass : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public string? WithdrawalReason { get; set; }
    public DateTime? WithdrawalDate { get; set; }
}

/// <summary>
/// Join table: many-to-many between Tutor and Course.
/// </summary>
public class TutorCourse : BaseEntity
{
    public Guid TutorId { get; set; }
    public Tutor Tutor { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public bool IsPrimary { get; set; } = true;  // Primary vs substitute tutor
}
