namespace TMS.Application.DTOs.Common;

/// <summary>
/// Aggregate data powering the admin dashboard.
/// </summary>
public class DashboardDto
{
    // Counts
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalTutors { get; set; }
    public int ActiveTutors { get; set; }
    public int TotalCourses { get; set; }
    public int ActiveClasses { get; set; }

    // Finance
    public decimal TotalRevenueThisMonth { get; set; }
    public decimal TotalRevenueThisYear { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public string Currency { get; set; } = "ZMW";

    // Attendance
    public double OverallAttendancePercentage { get; set; }

    // Recent activity
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
    public List<UpcomingClassDto> UpcomingClasses { get; set; } = new();
    public List<OverdueInvoiceDto> OverdueInvoices { get; set; } = new();
    public List<MonthlyEnrollmentDto> MonthlyEnrollments { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;   // "enrollment", "payment", "exam"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IconName { get; set; }
}

public class UpcomingClassDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TutorName { get; set; } = string.Empty;
    public DateTime NextSessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Room { get; set; }
    public int EnrolledCount { get; set; }
}

public class OverdueInvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal BalanceAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
}

public class MonthlyEnrollmentDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int NewEnrollments { get; set; }
    public int Withdrawals { get; set; }
}
