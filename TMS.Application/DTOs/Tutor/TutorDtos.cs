using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TMS.Domain.Enums;

namespace TMS.Application.DTOs.Tutor;

public class TutorDto
{
    public Guid Id { get; set; }
    public string StaffNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvailabilityNotes { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinDate { get; set; }
    public List<string> AssignedCourses { get; set; } = new();
    public int ActiveClassCount { get; set; }
}

public class TutorSummaryDto
{
    public Guid Id { get; set; }
    public string StaffNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public int ActiveClassCount { get; set; }
}

public class CreateTutorRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    [Required] public string Qualification { get; set; } = string.Empty;
    [Required] public string Specialization { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvailabilityNotes { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
}

public class UpdateTutorRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string PhoneNumber { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvailabilityNotes { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
}
