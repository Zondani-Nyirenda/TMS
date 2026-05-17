using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Auth;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class RefreshTokenRequest
{
    [Required] public string AccessToken { get; set; } = string.Empty;
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}

public class ChangePasswordRequest
{
    [Required] public string CurrentPassword { get; set; } = string.Empty;
    [Required, MinLength(8)] public string NewPassword { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
}
