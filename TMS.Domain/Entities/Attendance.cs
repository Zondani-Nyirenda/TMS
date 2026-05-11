using TMS.Domain.Common;
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// Tracks daily attendance for a student in a class session.
/// </summary>
public class Attendance : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? ArrivalTime { get; set; }   // Populated when Status = Late

    public string? Remarks { get; set; }

    // Who recorded the attendance
    public string? RecordedBy { get; set; }
    public bool IsSynced { get; set; } = true;    // false = recorded offline, pending sync
}
