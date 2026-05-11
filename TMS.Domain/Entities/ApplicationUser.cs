using Microsoft.AspNetCore.Identity;  // ✅ IdentityUser lives here
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// Extends ASP.NET Core IdentityUser with TMS-specific profile fields.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public UserRole Role { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Refresh token for JWT sliding expiry
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // PWA push subscription endpoint (stored as JSON)
    public string? PushSubscriptionJson { get; set; }

    // Navigation
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}