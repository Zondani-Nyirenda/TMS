using TMS.Domain.Common;
using TMS.Domain.Enums;
using TMS.Domain.ValueObjects;

namespace TMS.Domain.Entities;

/// <summary>
/// Represents a tutor who teaches one or more courses.
/// </summary>
public class Tutor : BaseEntity
{
    // Identity
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string StaffNumber { get; set; } = string.Empty;  // e.g. TUT-2024-001
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // Contact
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Address? Address { get; set; }

    // Professional
    public string Qualification { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? Bio { get; set; }

    // Availability (e.g. "Mon 08:00-16:00, Wed 08:00-12:00")
    public string? AvailabilityNotes { get; set; }

    // Navigation
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<TutorCourse> TutorCourses { get; set; } = new List<TutorCourse>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
