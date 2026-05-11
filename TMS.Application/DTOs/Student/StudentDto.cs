using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Student;

// ── Response ────────────────────────────────────────────────────────────────

public class StudentDto
{
    public Guid Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime EnrollmentDate { get; set; }

    // Guardian
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public string GuardianEmail { get; set; } = string.Empty;
    public string GuardianRelationship { get; set; } = string.Empty;

    // Address
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }

    // Summary stats (populated in detail view)
    public int ActiveClassCount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public double AttendancePercentage { get; set; }
}

public class StudentSummaryDto
{
    public Guid Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public int ActiveClassCount { get; set; }
    public decimal OutstandingBalance { get; set; }
}

// ── Commands ─────────────────────────────────────────────────────────────────

public class CreateStudentRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required] public string PhoneNumber { get; set; } = string.Empty;
    [Required] public DateTime DateOfBirth { get; set; }
    [Required] public Gender Gender { get; set; }

    // Guardian
    [Required] public string GuardianName { get; set; } = string.Empty;
    [Required] public string GuardianPhone { get; set; } = string.Empty;
    public string GuardianEmail { get; set; } = string.Empty;
    [Required] public string GuardianRelationship { get; set; } = string.Empty;

    // Address
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public string? Notes { get; set; }

    // Optionally create a login account at the same time
    public bool CreateLoginAccount { get; set; } = false;
    public string? LoginPassword { get; set; }
}

public class UpdateStudentRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    public string GuardianName { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public string GuardianEmail { get; set; } = string.Empty;
    public string GuardianRelationship { get; set; } = string.Empty;

    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class EnrollStudentRequest
{
    [Required] public Guid ClassId { get; set; }
}
