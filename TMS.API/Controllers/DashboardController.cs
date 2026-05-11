using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Returns role-specific dashboard data (counts, charts, recent activity).
/// </summary>
[Route("api/dashboard")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
        => _dashboard = dashboard;

    /// <summary>Admin dashboard — all system statistics.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken ct)
        => OkResult(await _dashboard.GetAdminDashboardAsync(ct));

    /// <summary>Tutor dashboard — their classes, attendance, upcoming exams.</summary>
    [HttpGet("tutor")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTutorDashboard(CancellationToken ct)
        => OkResult(await _dashboard.GetTutorDashboardAsync(CurrentUserId, ct));

    /// <summary>Student dashboard — timetable, attendance %, outstanding fees, results.</summary>
    [HttpGet("student")]
    [Authorize(Roles = "Admin,Student,Parent")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetStudentDashboard(CancellationToken ct)
        => OkResult(await _dashboard.GetStudentDashboardAsync(CurrentUserId, ct));
}