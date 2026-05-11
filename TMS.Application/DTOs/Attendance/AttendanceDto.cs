using System.ComponentModel.DataAnnotations;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Attendance;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    public string? Remarks { get; set; }
    public bool IsSynced { get; set; }
}

/// <summary>
/// Used to submit attendance for a full class in one request.
/// </summary>
public class SubmitAttendanceRequest
{
    [Required] public Guid ClassId { get; set; }
    [Required] public DateTime Date { get; set; }
    [Required, MinLength(1)] public List<StudentAttendanceEntry> Entries { get; set; } = new();
}

public class StudentAttendanceEntry
{
    [Required] public Guid StudentId { get; set; }
    [Required] public AttendanceStatus Status { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Summary row used in the attendance report table.
/// </summary>
public class AttendanceSummaryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public double AttendancePercentage =>
        TotalSessions == 0 ? 0 : Math.Round((Present + Late) * 100.0 / TotalSessions, 1);
}

/// <summary>
/// Offline attendance record saved locally in PWA before sync.
/// </summary>
public class OfflineAttendanceRecord
{
    public Guid LocalId { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public DateTime Date { get; set; }
    public List<StudentAttendanceEntry> Entries { get; set; } = new();
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;
}
