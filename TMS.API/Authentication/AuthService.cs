using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TMS.API.Authentication;
using TMS.Application.DTOs.Auth;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;

namespace TMS.API.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new NotFoundException("User", request.Email);

        if (!user.IsActive)
            throw new ForbiddenException("Your account has been deactivated. Contact admin.");

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            throw new ForbiddenException("Account is locked. Try again in 15 minutes.");

        if (!result.Succeeded)
            throw new DomainException("Invalid email or password.");

        return await BuildLoginResponseAsync(user);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    public async Task RegisterAsync(
        RegisterRequest request, CancellationToken ct = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = request.Role,
            IsActive = true,
            EmailConfirmed = true    // skip email confirmation for internal system
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new DomainException(string.Join(" | ", errors));
        }

        await _userManager.AddToRoleAsync(user, request.Role.ToString());
    }

    // ── Refresh token ─────────────────────────────────────────────────────────

    public async Task<LoginResponse> RefreshTokenAsync(
        RefreshTokenRequest request, CancellationToken ct = default)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken)
            ?? throw new DomainException("Invalid access token.");

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new DomainException("Invalid token claims.");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiry <= DateTime.UtcNow)
            throw new DomainException("Refresh token is invalid or expired. Please log in again.");

        return await BuildLoginResponseAsync(user);
    }

    // ── Change password ───────────────────────────────────────────────────────

    public async Task ChangePasswordAsync(
        string userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var result = await _userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new DomainException(string.Join(" | ", errors));
        }

        // Invalidate all existing refresh tokens on password change
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    // ── Revoke ────────────────────────────────────────────────────────────────

    public async Task RevokeTokenAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<LoginResponse> BuildLoginResponseAsync(ApplicationUser user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                IsActive = user.IsActive
            }
        };
    }
}