using TMS.Domain.Common;
using TMS.Domain.Enums;

namespace TMS.Domain.Entities;

/// <summary>
/// System notification sent to a user.
/// </summary>
public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Optional deep-link (e.g. to the relevant payment or exam)
    public string? ActionUrl { get; set; }

    // For push notifications
    public bool IsPushSent { get; set; } = false;
    public DateTime? PushSentAt { get; set; }
}
