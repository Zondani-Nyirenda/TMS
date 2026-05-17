using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.DTOs.StudyMaterial;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages subjects and their categories for the Study Materials module.
/// </summary>
[Route("api/subjects")]
public class SubjectsController : BaseController
{
    private readonly ISubjectService _subjects;

    public SubjectsController(ISubjectService subjects)
        => _subjects = subjects;

    // ── GET /api/subjects ─────────────────────────────────────────────────────

    /// <summary>All active subjects with category and material counts.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(List<SubjectSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var result = await _subjects.GetAllAsync(activeOnly, ct);
        return OkResult(result);
    }

    // ── GET /api/subjects/{id} ────────────────────────────────────────────────

    /// <summary>Subject detail with category and material counts.</summary>
    [HttpGet("{id:guid}", Name = "GetSubjectById")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(SubjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _subjects.GetByIdAsync(id, ct);
        return OkResult(result);
    }

    // ── POST /api/subjects ────────────────────────────────────────────────────

    /// <summary>Create a new subject.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SubjectDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubjectRequest request, CancellationToken ct)
    {
        var subject = await _subjects.CreateAsync(request, ct);
        return CreatedAtRoute("GetSubjectById",
            new { id = subject.Id },
            new { success = true, data = subject });
    }

    // ── PUT /api/subjects/{id} ────────────────────────────────────────────────

    /// <summary>Update an existing subject.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SubjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateSubjectRequest request, CancellationToken ct)
    {
        var result = await _subjects.UpdateAsync(id, request, ct);
        return OkResult(result, "Subject updated.");
    }

    // ── DELETE /api/subjects/{id} ─────────────────────────────────────────────

    /// <summary>Soft-delete a subject and all its categories/materials.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _subjects.DeleteAsync(id, ct);
        return OkResult(true, "Subject deleted.");
    }

    // ── GET /api/subjects/{id}/categories ─────────────────────────────────────

    /// <summary>All categories under a subject.</summary>
    [HttpGet("{id:guid}/categories")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(List<MaterialCategorySummaryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategories(Guid id, CancellationToken ct)
    {
        var result = await _subjects.GetCategoriesAsync(id, ct);
        return OkResult(result);
    }

    // ── POST /api/subjects/{id}/categories ────────────────────────────────────

    /// <summary>Add a new category to a subject.</summary>
    [HttpPost("{id:guid}/categories")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(MaterialCategoryDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateCategory(
        Guid id, [FromBody] CreateMaterialCategoryRequest request, CancellationToken ct)
    {
        // Ensure route id matches body
        request.SubjectId = id;

        var result = await _subjects.CreateCategoryAsync(request, ct);
        return OkResult(result, "Category created.");
    }

    // ── PUT /api/subjects/categories/{categoryId} ─────────────────────────────

    /// <summary>Update a category.</summary>
    [HttpPut("categories/{categoryId:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(MaterialCategoryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory(
        Guid categoryId,
        [FromBody] UpdateMaterialCategoryRequest request,
        CancellationToken ct)
    {
        var result = await _subjects.UpdateCategoryAsync(categoryId, request, ct);
        return OkResult(result, "Category updated.");
    }

    // ── DELETE /api/subjects/categories/{categoryId} ──────────────────────────

    /// <summary>Soft-delete a category and all its materials.</summary>
    [HttpDelete("categories/{categoryId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken ct)
    {
        await _subjects.DeleteCategoryAsync(categoryId, ct);
        return OkResult(true, "Category deleted.");
    }

    // ── POST /api/subjects/{id}/classes/{classId} ─────────────────────────────

    /// <summary>Assign a subject to a class group.</summary>
    [HttpPost("{id:guid}/classes/{classId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> AssignToClass(
        Guid id, Guid classId, CancellationToken ct)
    {
        await _subjects.AssignToClassAsync(id, classId, ct);
        return OkResult(true, "Subject assigned to class.");
    }

    // ── DELETE /api/subjects/{id}/classes/{classId} ───────────────────────────

    /// <summary>Remove a subject from a class group.</summary>
    [HttpDelete("{id:guid}/classes/{classId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveFromClass(
        Guid id, Guid classId, CancellationToken ct)
    {
        await _subjects.RemoveFromClassAsync(id, classId, ct);
        return OkResult(true, "Subject removed from class.");
    }
}