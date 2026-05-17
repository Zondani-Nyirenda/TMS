using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.DTOs.Auth;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

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
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return OkResult(response, "Login successful.");
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]                          // lock to [Authorize(Roles="Admin")] if needed
    [ProducesResponseType(201)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        await _authService.RegisterAsync(request, ct);
        return StatusCode(201, new { success = true, message = "User registered successfully." });
    }

    /// <summary>Refresh an expired access token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshTokenAsync(request, ct);
        return OkResult(response, "Token refreshed.");
    }

    /// <summary>Change password for the currently authenticated user.</summary>
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, request, ct);
        return OkResult(true, "Password changed successfully.");
    }

    /// <summary>Logout — revoke the refresh token server-side.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(CurrentUserId, ct);
        return OkResult(true, "Logged out successfully.");
    }

    /// <summary>Get the current user's profile from the JWT.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public IActionResult Me()
    {
        var user = new UserDto
        {
            Id = CurrentUserId,
            Email = CurrentUserEmail,
            FullName = $"{User.FindFirst("firstName")?.Value} {User.FindFirst("lastName")?.Value}".Trim(),
            IsActive = true
        };
        return OkResult(user);
    }
}