using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMS.Application.Common;

namespace TMS.API.Controllers;

/// <summary>
/// Base class for all TMS controllers.
/// Provides JWT user helpers and a consistent response envelope.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    // ── Current user helpers ──────────────────────────────────────────────────

    protected string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    protected bool IsAdmin => CurrentUserRole == "Admin";
    protected bool IsTutor => CurrentUserRole == "Tutor";
    protected bool IsStudent => CurrentUserRole == "Student";
    protected bool IsAccountant => CurrentUserRole == "Accountant";

    // ── Response helpers ──────────────────────────────────────────────────────

    protected IActionResult OkResult<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResult<T>(T data, string routeName, object routeValues)
        => CreatedAtRoute(routeName, routeValues, ApiResponse<T>.Ok(data));

    protected IActionResult FailResult(string error)
        => BadRequest(ApiResponse<object>.Fail(error));
}