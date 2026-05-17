using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.DTOs.StudyMaterial;
using TMS.Application.Interfaces;
using TMS.Domain.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages study materials — upload, browse, stream, download, bookmark,
/// and video progress tracking.
/// </summary>
[Route("api/study-materials")]
public class StudyMaterialsController : BaseController
{
    private readonly IStudyMaterialService _materials;
    private readonly IUnitOfWork _uow;

    public StudyMaterialsController(
        IStudyMaterialService materials,
        IUnitOfWork uow)
    {
        _materials = materials;
        _uow = uow;
    }

    // ── GET /api/study-materials ──────────────────────────────────────────────

    /// <summary>Paginated, filtered list of study materials.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(PagedResult<StudyMaterialSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MaterialFilterQuery query, CancellationToken ct)
    {
        var result = await _materials.GetAllAsync(query, ct);
        return OkResult(result);
    }

    // ── GET /api/study-materials/{id} ─────────────────────────────────────────

    /// <summary>Full material detail. If student, enriches with bookmark
    /// and video progress.</summary>
    [HttpGet("{id:guid}", Name = "GetMaterialById")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(StudyMaterialDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        Guid? studentId = null;

        if (IsStudent)
        {
            var student = await _uow.Students.FirstOrDefaultAsync(
                s => s.UserId == CurrentUserId, ct);
            studentId = student?.Id;
        }

        var result = await _materials.GetByIdAsync(id, studentId, ct);
        return OkResult(result);
    }

    // ── POST /api/study-materials ─────────────────────────────────────────────

    /// <summary>Upload a new study material (Admin/Tutor only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(StudyMaterialDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudyMaterialRequest request, CancellationToken ct)
    {
        var material = await _materials.CreateAsync(request, CurrentUserId, ct);
        return CreatedAtRoute("GetMaterialById",
            new { id = material.Id },
            new { success = true, data = material });
    }

    // ── PUT /api/study-materials/{id} ─────────────────────────────────────────

    /// <summary>Update material metadata or replace file.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(StudyMaterialDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateStudyMaterialRequest request, CancellationToken ct)
    {
        var result = await _materials.UpdateAsync(id, request, ct);
        return OkResult(result, "Material updated.");
    }

    // ── DELETE /api/study-materials/{id} ──────────────────────────────────────

    /// <summary>Soft-delete a material and remove its files.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _materials.DeleteAsync(id, ct);
        return OkResult(true, "Material deleted.");
    }

    // ── POST /api/study-materials/{id}/view ───────────────────────────────────

    /// <summary>Increment view count when a student opens a material.</summary>
    [HttpPost("{id:guid}/view")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RecordView(Guid id, CancellationToken ct)
    {
        await _materials.IncrementViewCountAsync(id, ct);
        return OkResult(true);
    }

    // ── POST /api/study-materials/{id}/download ───────────────────────────────

    /// <summary>Record a download and return the file URL.</summary>
    [HttpPost("{id:guid}/download")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RecordDownload(Guid id, CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        if (student is null && IsStudent)
            return FailResult("Student record not found.");

        var studentId = student?.Id ?? Guid.Empty;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _materials.RecordDownloadAsync(id, studentId, ipAddress, ct);

        var material = await _materials.GetByIdAsync(id, null, ct);
        return OkResult(new { fileUrl = material.FileUrl }, "Download recorded.");
    }

    // ── GET /api/study-materials/bookmarks ────────────────────────────────────

    /// <summary>All bookmarked materials for the current student.</summary>
    [HttpGet("bookmarks")]
    [Authorize(Roles = "Admin,Student")] // Add Admin here
    public async Task<IActionResult> GetBookmarks(CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        // If you're an Admin, you won't have a Student record. 
        // Return an empty list instead of a FailResult so the UI stays clean.
        if (student is null)
            return OkResult(new List<BookmarkDto>());

        var result = await _materials.GetBookmarksAsync(student.Id, ct);
        return OkResult(result);
    }

    // ── POST /api/study-materials/{id}/bookmark ───────────────────────────────

    /// <summary>Bookmark a material.</summary>
    [HttpPost("{id:guid}/bookmark")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(BookmarkDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> AddBookmark(Guid id, CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        if (student is null)
            return FailResult("Student record not found.");

        var result = await _materials.AddBookmarkAsync(id, student.Id, ct);
        return OkResult(result, "Material bookmarked.");
    }

    // ── DELETE /api/study-materials/{id}/bookmark ─────────────────────────────

    /// <summary>Remove a bookmark.</summary>
    [HttpDelete("{id:guid}/bookmark")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveBookmark(Guid id, CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        if (student is null)
            return FailResult("Student record not found.");

        await _materials.RemoveBookmarkAsync(id, student.Id, ct);
        return OkResult(true, "Bookmark removed.");
    }

    // ── GET /api/study-materials/{id}/progress ────────────────────────────────

    /// <summary>Get video progress for the current student.</summary>
    [HttpGet("{id:guid}/progress")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(VideoProgressDto), 200)]
    public async Task<IActionResult> GetProgress(Guid id, CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        if (student is null)
            return FailResult("Student record not found.");

        var result = await _materials.GetVideoProgressAsync(id, student.Id, ct);
        return OkResult(result);
    }

    // ── POST /api/study-materials/progress ────────────────────────────────────

    /// <summary>Save or update video watch progress (called every ~10 seconds
    /// while watching).</summary>
    [HttpPost("progress")]

    [ProducesResponseType(typeof(VideoProgressDto), 200)]
    public async Task<IActionResult> UpsertProgress(
        [FromBody] UpdateVideoProgressRequest request, CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        if (student is null)
            return FailResult("Student record not found.");

        // Always use the authenticated student's ID — never trust body
        request.StudentId = student.Id;

        var result = await _materials.UpsertVideoProgressAsync(request, ct);
        return OkResult(result);
    }

    // ── GET /api/study-materials/continue-watching ────────────────────────────

    /// <summary>In-progress videos for the current student — "Continue
    /// Watching" feature.</summary>
    [HttpGet("continue-watching")]
    [Authorize(Roles = "Admin,Student")] 
    public async Task<IActionResult> ContinueWatching(CancellationToken ct)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == CurrentUserId, ct);

        // Don't "Fail" if an Admin visits the page. Just show nothing.
        if (student is null)
            return OkResult(new List<VideoProgressDto>());

        var result = await _materials.GetContinueWatchingAsync(student.Id, ct);
        return OkResult(result);
    }
}