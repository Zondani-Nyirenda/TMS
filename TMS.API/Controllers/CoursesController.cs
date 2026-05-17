using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.DTOs.Course;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

// ── Courses ───────────────────────────────────────────────────────────────────

[Route("api/courses")]
public class CoursesController : BaseController
{
    private readonly ICourseService _courses;

    public CoursesController(ICourseService courses) => _courses = courses;

    /// <summary>All courses with optional search/filter.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tutor,Accountant,Student")]
    [ProducesResponseType(typeof(PagedResult<CourseDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationQuery query, CancellationToken ct)
        => OkResult(await _courses.GetAllAsync(query, ct));

    /// <summary>Course detail.</summary>
    [HttpGet("{id:guid}", Name = "GetCourseById")]
    [Authorize(Roles = "Admin,Tutor,Accountant,Student")]
    [ProducesResponseType(typeof(CourseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await _courses.GetByIdAsync(id, ct));

    /// <summary>Create a new course.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CourseDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { message = "Validation failed", errors });
        }

        try
        {
            var course = await _courses.CreateAsync(request, ct);
            return CreatedAtRoute("GetCourseById", new { id = course.Id },
                new { success = true, data = course });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update course details.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CourseDto), 200)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateCourseRequest request, CancellationToken ct)
        => OkResult(await _courses.UpdateAsync(id, request, ct), "Course updated.");

    /// <summary>Soft-delete a course.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _courses.DeleteAsync(id, ct);
        return OkResult(true, "Course deleted.");
    }
}

// ── Classes ───────────────────────────────────────────────────────────────────

[Route("api/classes")]
public class ClassesController : BaseController
{
    private readonly IClassService _classes;

    public ClassesController(IClassService classes) => _classes = classes;

    /// <summary>All scheduled classes.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tutor,Accountant,Student")]
    [ProducesResponseType(typeof(PagedResult<ClassDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationQuery query, CancellationToken ct)
        => OkResult(await _classes.GetAllAsync(query, ct));

    /// <summary>Class detail.</summary>
    [HttpGet("{id:guid}", Name = "GetClassById")]
    [Authorize(Roles = "Admin,Tutor,Accountant,Student")]
    [ProducesResponseType(typeof(ClassDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await _classes.GetByIdAsync(id, ct));

    /// <summary>Create a new class schedule.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ClassDto), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClassRequest request, CancellationToken ct)
    {
        var cls = await _classes.CreateAsync(request, ct);
        return CreatedAtRoute("GetClassById", new { id = cls.Id },
            new { success = true, data = cls });
    }

    /// <summary>Update class details.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ClassDto), 200)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateClassRequest request, CancellationToken ct)
        => OkResult(await _classes.UpdateAsync(id, request, ct), "Class updated.");

    /// <summary>Soft-delete a class.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _classes.DeleteAsync(id, ct);
        return OkResult(true, "Class deleted.");
    }

    /// <summary>All students enrolled in a class.</summary>
    [HttpGet("{id:guid}/students")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetStudents(Guid id, CancellationToken ct)
        => OkResult(await _classes.GetEnrolledStudentsAsync(id, ct));
}