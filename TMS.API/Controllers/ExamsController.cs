using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.Application.Common;
using TMS.Application.DTOs.Exam;
using TMS.Application.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Manages exams, result entry, grade publishing, and performance reporting.
/// </summary>
[Route("api/exams")]
public class ExamsController : BaseController
{
    private readonly IExamService _exams;

    public ExamsController(IExamService exams) => _exams = exams;

    /// <summary>Paginated list of exams. Filter by classId via query string.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(PagedResult<ExamDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationQuery query,
        [FromQuery] Guid? classId,
        CancellationToken ct)
        => OkResult(await _exams.GetAllAsync(query, classId, ct));

    /// <summary>Exam detail with aggregate statistics.</summary>
    [HttpGet("{id:guid}", Name = "GetExamById")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(ExamDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await _exams.GetByIdAsync(id, ct));

    /// <summary>Create an exam for a class.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(ExamDto), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExamRequest request, CancellationToken ct)
    {
        var exam = await _exams.CreateAsync(request, ct);
        return CreatedAtRoute("GetExamById", new { id = exam.Id },
            new { success = true, data = exam });
    }

    /// <summary>Update exam details.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(typeof(ExamDto), 200)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateExamRequest request, CancellationToken ct)
        => OkResult(await _exams.UpdateAsync(id, request, ct), "Exam updated.");

    /// <summary>Delete an exam (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _exams.DeleteAsync(id, ct);
        return OkResult(true, "Exam deleted.");
    }

    /// <summary>Bulk-submit marks for all students in an exam.</summary>
    [HttpPost("{id:guid}/results")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> SubmitResults(
        Guid id, [FromBody] SubmitResultsRequest request, CancellationToken ct)
    {
        request.ExamId = id;  // ensure route id matches body
        await _exams.SubmitResultsAsync(request, CurrentUserId, ct);
        return OkResult(true, $"Results saved for {request.Entries.Count} student(s).");
    }

    /// <summary>Get all results for an exam.</summary>
    [HttpGet("{id:guid}/results")]
    [Authorize(Roles = "Admin,Tutor,Student")]
    [ProducesResponseType(typeof(List<ResultDto>), 200)]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken ct)
        => OkResult(await _exams.GetResultsAsync(id, ct));

    /// <summary>Publish exam results so students can view them.</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,Tutor")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await _exams.PublishResultsAsync(id, ct);
        return OkResult(true, "Results published to students.");
    }

    /// <summary>Student performance report for a specific class.</summary>
    [HttpGet("performance")]
    [Authorize(Roles = "Admin,Tutor,Student,Parent")]
    [ProducesResponseType(typeof(StudentPerformanceDto), 200)]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] Guid studentId,
        [FromQuery] Guid classId,
        CancellationToken ct)
        => OkResult(await _exams.GetStudentPerformanceAsync(studentId, classId, ct));
}