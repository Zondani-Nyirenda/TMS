using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.DTOs.Content;

namespace TMS.Application.Interfaces
{
    public interface IContentService
    {
        // ── Modules ───────────────────────────────────────────────────────────────
        Task<List<ContentModuleDto>> GetModulesAsync(Guid courseId, CancellationToken ct = default);
        Task<ContentModuleDto> GetModuleByIdAsync(Guid id, CancellationToken ct = default);
        Task<ContentModuleDto> CreateModuleAsync(CreateContentModuleRequest request, string createdByUserId, CancellationToken ct = default);
        Task<ContentModuleDto> UpdateModuleAsync(Guid id, UpdateContentModuleRequest request, CancellationToken ct = default);
        Task DeleteModuleAsync(Guid id, CancellationToken ct = default);
        Task ReorderModulesAsync(Guid courseId, List<Guid> orderedIds, CancellationToken ct = default);

        // ── Items ─────────────────────────────────────────────────────────────────
        Task<ContentItemDto> GetItemByIdAsync(Guid id, string? requestingUserId, CancellationToken ct = default);
        Task<ContentItemDto> CreateItemAsync(CreateContentItemRequest request, Stream? fileStream,
                                                  string? fileName, string? contentType,
                                                  string uploadedByUserId, CancellationToken ct = default);
        Task<ContentItemDto> UpdateItemAsync(Guid id, UpdateContentItemRequest request, CancellationToken ct = default);
        Task DeleteItemAsync(Guid id, CancellationToken ct = default);
        Task ReorderItemsAsync(Guid moduleId, List<Guid> orderedIds, CancellationToken ct = default);

        // ── File delivery ─────────────────────────────────────────────────────────
        /// <summary>
        /// Verify access, record progress, and return a short-lived download URL.
        /// </summary>
        Task<string> GetSecureDownloadUrlAsync(Guid itemId, string studentUserId, CancellationToken ct = default);

        /// <summary>
        /// Stream the file bytes directly (for inline video/audio players).
        /// Returns (stream, contentType, fileName).
        /// </summary>
        Task<(Stream Stream, string ContentType, string FileName)>
                                  StreamFileAsync(Guid itemId, string requestingUserId, CancellationToken ct = default);

        // ── Progress ──────────────────────────────────────────────────────────────
        Task MarkProgressAsync(MarkProgressRequest request, CancellationToken ct = default);
        Task<CourseContentDto> GetCourseContentAsync(Guid courseId, string? studentUserId, CancellationToken ct = default);

        // ── Access grants (Restricted items) ─────────────────────────────────────
        Task GrantAccessAsync(GrantAccessRequest request, string grantedByUserId, CancellationToken ct = default);
        Task RevokeAccessAsync(Guid contentItemId, Guid studentId, CancellationToken ct = default);
        Task<bool> HasAccessAsync(Guid contentItemId, string userId, CancellationToken ct = default);
    }
}
