using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.DTOs.Content;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages course content: modules, items, file upload/download, and student progress.
///
/// Route structure:
///   GET  api/content/courses/{courseId}          — full course outline
///   GET/POST/PUT/DELETE api/content/modules/...  — module CRUD
///   GET/POST/PUT/DELETE api/content/items/...    — item CRUD + file upload
///   GET  api/content/items/{id}/download         — secure download URL
///   GET  api/content/items/{id}/stream           — byte-range streaming
///   POST api/content/items/{id}/progress         — mark viewed / save position
///   POST api/content/items/{id}/access           — grant restricted access
///   DELETE api/content/items/{id}/access/{sid}   — revoke access
/// </summary>
[Route("api/content")]
public class ContentController : BaseController
{
    private readonly IContentService _content;

    public ContentController(IContentService content) => _content = content;

    // ══════════════════════════════════════════════════════════════════════════
    //  Course outline
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Full content outline for a course with per-student progress.</summary>
    [HttpGet("courses/{courseId:guid}")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(CourseContentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCourseContent(Guid courseId, CancellationToken ct)
    {
        var dto = await _content.GetCourseContentAsync(courseId, CurrentUserId, ct);
        return OkResult(dto);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Modules
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>All modules for a course (ordered by SortOrder).</summary>
    [HttpGet("courses/{courseId:guid}/modules")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(List<ContentModuleDto>), 200)]
    public async Task<IActionResult> GetModules(Guid courseId, CancellationToken ct)
        => OkResult(await _content.GetModulesAsync(courseId, ct));

    /// <summary>Single module detail.</summary>
    [HttpGet("modules/{id:guid}", Name = "GetContentModuleById")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(ContentModuleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetModule(Guid id, CancellationToken ct)
        => OkResult(await _content.GetModuleByIdAsync(id, ct));

    /// <summary>Create a new content module inside a course.</summary>
    [HttpPost("modules")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(ContentModuleDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateModule(
        [FromBody] CreateContentModuleRequest request, CancellationToken ct)
    {
        var dto = await _content.CreateModuleAsync(request, CurrentUserId, ct);
        return CreatedAtRoute("GetContentModuleById", new { id = dto.Id },
            new { success = true, data = dto });
    }

    /// <summary>Update module title, description, order or status.</summary>
    [HttpPut("modules/{id:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(ContentModuleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateModule(
        Guid id, [FromBody] UpdateContentModuleRequest request, CancellationToken ct)
        => OkResult(await _content.UpdateModuleAsync(id, request, ct), "Module updated.");

    /// <summary>Soft-delete a module and all its items.</summary>
    [HttpDelete("modules/{id:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteModule(Guid id, CancellationToken ct)
    {
        await _content.DeleteModuleAsync(id, ct);
        return OkResult(true, "Module deleted.");
    }

    /// <summary>Save a new display order for all modules in a course.</summary>
    [HttpPost("courses/{courseId:guid}/modules/reorder")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ReorderModules(
        Guid courseId, [FromBody] List<Guid> orderedIds, CancellationToken ct)
    {
        await _content.ReorderModulesAsync(courseId, orderedIds, ct);
        return OkResult(true, "Modules reordered.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Items — metadata CRUD
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Get a single content item with download URL.</summary>
    [HttpGet("items/{id:guid}", Name = "GetContentItemById")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(ContentItemDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItem(Guid id, CancellationToken ct)
        => OkResult(await _content.GetItemByIdAsync(id, CurrentUserId, ct));

    /// <summary>
    /// Create a content item.  For file-based types, send as multipart/form-data
    /// with the file in the "file" field and JSON metadata in "request".
    /// For Link type, send JSON only.
    /// </summary>
    [HttpPost("items")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(ContentItemDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    [RequestSizeLimit(500 * 1024 * 1024)]   // 500 MB cap
    public async Task<IActionResult> CreateItem(
        [FromForm] string requestJson,
        IFormFile? file,
        CancellationToken ct)
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<CreateContentItemRequest>(
            requestJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new ArgumentException("Invalid request JSON.");

        Stream? stream = null;
        string? fileName = null;
        string? contentType = null;

        if (file is not null)
        {
            stream = file.OpenReadStream();
            fileName = file.FileName;
            contentType = file.ContentType;
        }

        var dto = await _content.CreateItemAsync(
            request, stream, fileName, contentType, CurrentUserId, ct);

        return CreatedAtRoute("GetContentItemById", new { id = dto.Id },
            new { success = true, data = dto });
    }

    /// <summary>Update item metadata (not the file itself).</summary>
    [HttpPut("items/{id:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(ContentItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateItem(
        Guid id, [FromBody] UpdateContentItemRequest request, CancellationToken ct)
        => OkResult(await _content.UpdateItemAsync(id, request, ct), "Item updated.");

    /// <summary>Soft-delete a content item and its physical file.</summary>
    [HttpDelete("items/{id:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken ct)
    {
        await _content.DeleteItemAsync(id, ct);
        return OkResult(true, "Item deleted.");
    }

    /// <summary>Save a new display order for all items in a module.</summary>
    [HttpPost("modules/{moduleId:guid}/items/reorder")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ReorderItems(
        Guid moduleId, [FromBody] List<Guid> orderedIds, CancellationToken ct)
    {
        await _content.ReorderItemsAsync(moduleId, orderedIds, ct);
        return OkResult(true, "Items reordered.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  File delivery
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a short-lived download URL for a content item.
    /// Students: access control enforced.  Tutors/Admins: always allowed.
    /// </summary>
    [HttpGet("items/{id:guid}/download")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken ct)
    {
        var item = await _content.GetItemByIdAsync(id, CurrentUserId, ct);

        // Build absolute URL pointing to the API stream endpoint
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var streamUrl = $"{baseUrl}/api/content/items/{id}/stream";

        return OkResult(new { url = streamUrl });
    }

    /// <summary>
    /// Streams a file with byte-range support (supports inline video/audio players).
    /// </summary>
    [HttpGet("items/{id:guid}/stream")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(200)]
    [ProducesResponseType(206)]   // Partial Content
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Stream(Guid id, CancellationToken ct)
    {
        var (stream, contentType, fileName) =
            await _content.StreamFileAsync(id, CurrentUserId, ct);

        // Let ASP.NET Core handle Range headers (video seeking)
        return File(stream, contentType, fileName, enableRangeProcessing: true);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Student progress
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Mark a content item as viewed or completed, and save video position.</summary>
    [HttpPost("items/{id:guid}/progress")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> MarkProgress(
        Guid id, [FromBody] MarkProgressRequest request, CancellationToken ct)
    {
        request.ContentItemId = id;   // enforce route id
        await _content.MarkProgressAsync(request, ct);
        return OkResult(true, "Progress recorded.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Access control (Restricted items)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Grant a student access to a Restricted item.</summary>
    [HttpPost("items/{id:guid}/access")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GrantAccess(
        Guid id, [FromBody] GrantAccessRequest request, CancellationToken ct)
    {
        request.ContentItemId = id;
        await _content.GrantAccessAsync(request, CurrentUserId, ct);
        return OkResult(true, "Access granted.");
    }

    /// <summary>Revoke a student's access to a Restricted item.</summary>
    [HttpDelete("items/{id:guid}/access/{studentId:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RevokeAccess(
        Guid id, Guid studentId, CancellationToken ct)
    {
        await _content.RevokeAccessAsync(id, studentId, ct);
        return OkResult(true, "Access revoked.");
    }
}