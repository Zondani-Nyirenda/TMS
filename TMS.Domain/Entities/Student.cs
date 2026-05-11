using System.Net;
using System.Reflection;
using TMS.Domain.Common;
using TMS.Domain.Enums;
using TMS.Domain.ValueObjects;

namespace TMS.Domain.Entities;

/// <summary>
/// Represents a student enrolled in the tuition centre.
/// </summary>
public class Student : BaseEntity
{
    // Identity
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string StudentNumber { get; set; } = string.Empty;  // e.g. STU-2024-001
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // Contact
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Address? Address { get; set; }

    // Guardian / Parent
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public string GuardianEmail { get; set; } = string.Empty;
    public string GuardianRelationship { get; set; } = string.Empty;

    // Academic
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation: linked Identity user (optional — student may not have a login)
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Navigation collections
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Result> Results { get; set; } = new List<Result>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    // Computed helpers (not mapped)
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year -
                      (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}
