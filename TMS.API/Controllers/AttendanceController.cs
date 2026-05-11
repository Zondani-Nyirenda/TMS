using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.DTOs.Attendance;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages attendance recording, reporting, and offline sync.
/// </summary>
[Route("api/attendance")]
public class AttendanceController : BaseController
{
    private readonly IAttendanceService _attendance;

    public AttendanceController(IAttendanceService attendance)
        => _attendance = attendance;

    // ── GET /api/attendance/class/{classId}?date=2024-01-15 ──────────────────

    /// <summary>Get attendance records for a class on a specific date.</summary>
    [HttpGet("class/{classId:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(List<AttendanceDto>), 200)]
    public async Task<IActionResult> GetByClassAndDate(
        Guid classId,
        [FromQuery] DateTime date,
        CancellationToken ct)
    {
        var records = await _attendance.GetByClassAndDateAsync(classId, date, ct);
        return OkResult(records);
    }

    // ── POST /api/attendance ──────────────────────────────────────────────────

    /// <summary>Submit attendance for a full class session.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitAttendanceRequest request,
        CancellationToken ct)
    {
        await _attendance.SubmitAsync(request, CurrentUserEmail, ct);
        return OkResult(true, $"Attendance recorded for {request.Entries.Count} student(s).");
    }

    // ── GET /api/attendance/summary/{classId}?from=&to= ───────────────────────

    /// <summary>
    /// Attendance summary (present/absent/late counts and %)
    /// for all students in a class over a date range.
    /// </summary>
    [HttpGet("summary/{classId:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(List<AttendanceSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary(
        Guid classId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var summary = await _attendance.GetSummaryAsync(classId, from, to, ct);
        return OkResult(summary);
    }

    // ── POST /api/attendance/sync ─────────────────────────────────────────────

    /// <summary>
    /// Sync offline attendance records collected while the device was offline.
    /// PWA calls this when connectivity is restored.
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> SyncOffline(
        [FromBody] List<OfflineAttendanceRecord> records,
        CancellationToken ct)
    {
        await _attendance.SyncOfflineRecordsAsync(records, CurrentUserEmail, ct);
        return OkResult(true, $"Synced {records.Count} offline record(s).");
    }
}