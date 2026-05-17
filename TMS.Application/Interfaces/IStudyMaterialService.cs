using TMS.Application.Common;
using TMS.Application.DTOs.StudyMaterial;

namespace TMS.Application.Interfaces;

public interface IStudyMaterialService
{
    // Browse
    Task<PagedResult<StudyMaterialSummaryDto>> GetAllAsync(
        MaterialFilterQuery query, CancellationToken ct = default);

    Task<StudyMaterialDto> GetByIdAsync(
        Guid id, Guid? studentId = null, CancellationToken ct = default);

    // Admin / Tutor
    Task<StudyMaterialDto> CreateAsync(
        CreateStudyMaterialRequest request, string uploadedByUserId, CancellationToken ct = default);

    Task<StudyMaterialDto> UpdateAsync(
        Guid id, UpdateStudyMaterialRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // Student interactions
    Task IncrementViewCountAsync(Guid materialId, CancellationToken ct = default);
    Task RecordDownloadAsync(Guid materialId, Guid studentId, string? ipAddress = null, CancellationToken ct = default);

    // Bookmarks
    Task<List<BookmarkDto>> GetBookmarksAsync(Guid studentId, CancellationToken ct = default);
    Task<BookmarkDto> AddBookmarkAsync(Guid materialId, Guid studentId, CancellationToken ct = default);
    Task RemoveBookmarkAsync(Guid materialId, Guid studentId, CancellationToken ct = default);

    // Video progress
    Task<VideoProgressDto?> GetVideoProgressAsync(Guid materialId, Guid studentId, CancellationToken ct = default);
    Task<VideoProgressDto> UpsertVideoProgressAsync(UpdateVideoProgressRequest request, CancellationToken ct = default);
    Task<List<VideoProgressDto>> GetContinueWatchingAsync(Guid studentId, CancellationToken ct = default);
}