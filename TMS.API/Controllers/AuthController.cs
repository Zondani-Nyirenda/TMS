using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.DTOs.Auth;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Handles authentication — login, register, token refresh, password changes.
/// </summary>
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
        => _authService = authService;

    /// <summary>Login and receive JWT access + refresh tokens.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return OkResult(response, "Login successful.");
    }

    /// <summary>Register a new user account (Admin only).</summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _authService.RegisterAsync(request, ct);
        return StatusCode(201, new { success = true, message = "User registered successfully." });
    }

    /// <summary>Use a valid refresh token to get a new access token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshTokenAsync(request, ct);
        return OkResult(response, "Token refreshed.");
    }

    /// <summary>Change password for the currently authenticated user.</summary>
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, request, ct);
        return OkResult(true, "Password changed successfully.");
    }

    /// <summary>Logout — revokes the refresh token server-side.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(CurrentUserId, ct);
        return OkResult(true, "Logged out successfully.");
    }

    /// <summary>Returns the current user's profile from the JWT.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public IActionResult Me()
    {
        var user = new UserDto
        {
            Id = CurrentUserId,
            Email = CurrentUserEmail,
            FullName = User.FindFirst("firstName")?.Value + " " +
                       User.FindFirst("lastName")?.Value,
            IsActive = true
        };
        return OkResult(user);
    }
}