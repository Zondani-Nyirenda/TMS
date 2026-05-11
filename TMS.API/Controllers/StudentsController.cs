using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.DTOs.Student;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages student records, enrollment, and class assignments.
/// </summary>
[Route("api/students")]
public class StudentsController : BaseController
{
    private readonly IStudentService _students;

    public StudentsController(IStudentService students)
        => _students = students;

    // ── GET /api/students ─────────────────────────────────────────────────────

    /// <summary>Paginated list of students with optional search.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tutor,Accountant")]
    [ProducesResponseType(typeof(PagedResult<StudentSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await _students.GetAllAsync(query, ct);
        return OkResult(result);
    }

    // ── GET /api/students/{id} ────────────────────────────────────────────────

    /// <summary>Full student detail including stats.</summary>
    [HttpGet("{id:guid}", Name = "GetStudentById")]
    [Authorize(Roles = "Admin,Tutor,Accountant,Student,Parent")]
    [ProducesResponseType(typeof(StudentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        // Students may only view their own record
        if (IsStudent)
        {
            var classes = await _students.GetStudentClassesAsync(id, ct);
            // TODO: enforce StudentId == CurrentUser link check
        }

        var student = await _students.GetByIdAsync(id, ct);
        return OkResult(student);
    }

    // ── POST /api/students ────────────────────────────────────────────────────

    /// <summary>Create a new student record.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(StudentDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudentRequest request, CancellationToken ct)
    {
        var student = await _students.CreateAsync(request, ct);
        return CreatedAtRoute("GetStudentById", new { id = student.Id },
            new { success = true, data = student });
    }

    // ── PUT /api/students/{id} ────────────────────────────────────────────────

    /// <summary>Update student details.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(typeof(StudentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateStudentRequest request, CancellationToken ct)
    {
        var student = await _students.UpdateAsync(id, request, ct);
        return OkResult(student, "Student updated.");
    }

    // ── DELETE /api/students/{id} ─────────────────────────────────────────────

    /// <summary>Soft-delete a student record.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _students.DeleteAsync(id, ct);
        return OkResult(true, "Student deleted.");
    }

    // ── POST /api/students/{id}/enroll ────────────────────────────────────────

    /// <summary>Enroll a student into a class.</summary>
    [HttpPost("{id:guid}/enroll")]
    [Authorize(Roles = "Admin,Accountant")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Enroll(
        Guid id, [FromBody] EnrollStudentRequest request, CancellationToken ct)
    {
        await _students.EnrollInClassAsync(id, request.ClassId, ct);
        return OkResult(true, "Student enrolled successfully.");
    }

    // ── POST /api/students/{id}/withdraw ──────────────────────────────────────

    /// <summary>Withdraw a student from a class.</summary>
    [HttpPost("{id:guid}/withdraw/{classId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Withdraw(
        Guid id, Guid classId,
        [FromQuery] string reason = "Withdrawn by admin",
        CancellationToken ct = default)
    {
        await _students.WithdrawFromClassAsync(id, classId, reason, ct);
        return OkResult(true, "Student withdrawn from class.");
    }

    // ── GET /api/students/{id}/classes ────────────────────────────────────────

    /// <summary>All classes a student is currently enrolled in.</summary>
    [HttpGet("{id:guid}/classes")]
    [Authorize(Roles = "Admin,Tutor,Accountant,Student,Parent")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetClasses(Guid id, CancellationToken ct)
    {
        var classes = await _students.GetStudentClassesAsync(id, ct);
        return OkResult(classes);
    }
}