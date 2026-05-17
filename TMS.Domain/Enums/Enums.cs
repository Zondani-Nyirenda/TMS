namespace TMS.Domain.Enums;

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    Excused
}

public enum PaymentStatus
{
    Paid,
    Partial,
    Due,
    Overdue,
    Cancelled,
    Refunded
}

public enum PaymentMethod
{
    Cash,
    BankTransfer,
    CreditCard,
    DebitCard,
    MobileMoney,
    Cheque
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public enum CourseStatus
{
    Active,
    Inactive,
    Archived
}

public enum ClassStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public enum EnrollmentStatus
{
    Active,
    Withdrawn,
    Completed,
    Suspended
}

public enum ExamType
{
    Quiz,
    MidTerm,
    FinalExam,
    Assignment,
    Practical,
    Oral
}

public enum GradeLevel
{
    APlus,
    A,
    AMinus,
    BPlus,
    B,
    BMinus,
    CPlus,
    C,
    CMinus,
    D,
    F
}

public enum NotificationType
{
    FeeReminder,
    ClassSchedule,
    ExamResult,
    GeneralAnnouncement,
    AttendanceAlert,
    PaymentConfirmation
}

public enum UserRole
{
    Admin,
    Tutor,
    Accountant,
    Student,
    Parent
}

public enum DayOfWeekEnum
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

// ── Course Content ────────────────────────────────────────────────────────────

/// <summary>Top-level grouping inside a course (e.g. "Week 1", "Module 2").</summary>
public enum ContentModuleStatus
{
    Draft,
    Published,
    Archived
}

/// <summary>The kind of resource attached to a content item.</summary>
public enum ResourceType
{
    Document,   // PDF, Word, PowerPoint, etc.
    Video,      // MP4, MOV streamed from server
    Audio,      // MP3, WAV
    Image,      // PNG, JPG, GIF
    Link,       // External URL
    Archive,    // ZIP, RAR
    Other
}

/// <summary>Controls who may access a resource.</summary>
public enum ResourceAccessLevel
{
    /// <summary>Visible to all enrolled students in the course.</summary>
    AllStudents,
    /// <summary>Visible only to tutors and admins.</summary>
    TutorsOnly,
    /// <summary>Restricted — access explicitly granted per student.</summary>
    Restricted
}

public enum MaterialType
{
    Book,
    Notes,
    VideoLesson,
    PastPaper,
    MarkingScheme,
    Assignment,
    Quiz,
    AudioLecture,
    Presentation,
    Other
}

public enum MaterialFileType
{
    Pdf,
    Video,
    Image,
    Audio,
    Zip,

}

public enum MaterialAccessLevel
{
    AllStudents,
    PremiumOnly,
    TeachersOnly,
 
}