using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.DTOs.Tutor;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

[Route("api/tutors")]
public class TutorsController : BaseController
{
    private readonly ITutorService _tutors;

    public TutorsController(ITutorService tutors) => _tutors = tutors;

    /// <summary>Paginated list of tutors.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<TutorSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationQuery query, CancellationToken ct)
        => OkResult(await _tutors.GetAllAsync(query, ct));

    /// <summary>Tutor detail.</summary>
    [HttpGet("{id:guid}", Name = "GetTutorById")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(TutorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await _tutors.GetByIdAsync(id, ct));

    /// <summary>Create a tutor (also creates a login account).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TutorDto), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTutorRequest request, CancellationToken ct)
    {
        var tutor = await _tutors.CreateAsync(request, ct);
        return CreatedAtRoute("GetTutorById", new { id = tutor.Id },
            new { success = true, data = tutor });
    }

    /// <summary>Update tutor details.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TutorDto), 200)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateTutorRequest request, CancellationToken ct)
        => OkResult(await _tutors.UpdateAsync(id, request, ct), "Tutor updated.");

    /// <summary>Soft-delete a tutor.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _tutors.DeleteAsync(id, ct);
        return OkResult(true, "Tutor deleted.");
    }

    /// <summary>Assign a tutor to a course.</summary>
    [HttpPost("{id:guid}/assign-course/{courseId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AssignCourse(
        Guid id, Guid courseId, CancellationToken ct)
    {
        await _tutors.AssignToCourseAsync(id, courseId, ct);
        return OkResult(true, "Tutor assigned to course.");
    }

    /// <summary>Remove a tutor from a course.</summary>
    [HttpDelete("{id:guid}/assign-course/{courseId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RemoveCourse(
        Guid id, Guid courseId, CancellationToken ct)
    {
        await _tutors.RemoveFromCourseAsync(id, courseId, ct);
        return OkResult(true, "Tutor removed from course.");
    }
}