using AutoMapper;
using TMS.Application.DTOs.Content;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class ContentService : IContentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public ContentService(
        IUnitOfWork uow,
        IMapper mapper,
        IFileStorageService storage)
    {
        _uow = uow;
        _mapper = mapper;
        _storage = storage;
    }

    // ── Modules ───────────────────────────────────────────────────────────────

    public async Task<List<ContentModuleDto>> GetModulesAsync(Guid courseId, CancellationToken ct = default)
    {
        var modules = await _uow.ContentModules.FindAsync(m => m.CourseId == courseId, ct);
        return _mapper.Map<List<ContentModuleDto>>(modules.OrderBy(m => m.SortOrder).ThenBy(m => m.CreatedAt));
    }

    public async Task<ContentModuleDto> GetModuleByIdAsync(Guid id, CancellationToken ct = default)
    {
        var module = await _uow.ContentModules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContentModule), id);
        return _mapper.Map<ContentModuleDto>(module);
    }

    public async Task<ContentModuleDto> CreateModuleAsync(CreateContentModuleRequest request, string createdByUserId, CancellationToken ct = default)
    {
        _ = await _uow.Courses.GetByIdAsync(request.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);

        var module = new ContentModule
        {
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            SortOrder = request.SortOrder,
            Status = ContentModuleStatus.Draft,
            CreatedBy = createdByUserId
        };
        await _uow.ContentModules.AddAsync(module, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ContentModuleDto>(module);
    }

    public async Task<ContentModuleDto> UpdateModuleAsync(Guid id, UpdateContentModuleRequest request, CancellationToken ct = default)
    {
        var module = await _uow.ContentModules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContentModule), id);
        module.Title = request.Title.Trim();
        module.SortOrder = request.SortOrder;
        module.Status = request.Status;
        await _uow.ContentModules.UpdateAsync(module, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ContentModuleDto>(module);
    }

    public async Task DeleteModuleAsync(Guid id, CancellationToken ct = default)
    {
        var module = await _uow.ContentModules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContentModule), id);
        var items = await _uow.ContentItems.FindAsync(i => i.ContentModuleId == id, ct);
        foreach (var item in items) await DeleteItemInternalAsync(item, ct);
        await _uow.ContentModules.DeleteAsync(module, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ReorderModulesAsync(Guid courseId, List<Guid> orderedIds, CancellationToken ct = default)
    {
        var modules = await _uow.ContentModules.FindAsync(m => m.CourseId == courseId, ct);
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var module = modules.FirstOrDefault(m => m.Id == orderedIds[i]);
            if (module != null)
            {
                module.SortOrder = i;
                await _uow.ContentModules.UpdateAsync(module, ct);
            }
        }
        await _uow.SaveChangesAsync(ct);
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    public async Task<ContentItemDto> GetItemByIdAsync(Guid id, string? requestingUserId, CancellationToken ct = default)
    {
        var item = await _uow.ContentItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContentItem), id);

        var dto = _mapper.Map<ContentItemDto>(item);

        if (item.ResourceType != ResourceType.Link && !string.IsNullOrEmpty(item.StoragePath))
            dto.DownloadUrl = $"api/content/items/{item.Id}/stream";

        return dto;
    }

    public async Task<string> GetSecureDownloadUrlAsync(Guid itemId, string studentUserId, CancellationToken ct = default)
    {
        var item = await _uow.ContentItems.GetByIdAsync(itemId, ct)
            ?? throw new NotFoundException(nameof(ContentItem), itemId);

        if (string.IsNullOrEmpty(item.StoragePath))
            throw new DomainException("No file path recorded.");

        return $"api/content/items/{item.Id}/stream";
    }

    public async Task<ContentItemDto> CreateItemAsync(
        CreateContentItemRequest request,
        Stream? fileStream, string? fileName, string? contentType,
        string uploadedByUserId, CancellationToken ct = default)
    {
        var module = await _uow.ContentModules.GetByIdAsync(request.ContentModuleId, ct)
            ?? throw new NotFoundException(nameof(ContentModule), request.ContentModuleId);

        string? storagePath = null;
        long? fileSize = null;

        if (fileStream != null && !string.IsNullOrEmpty(fileName))
        {
            var folder = $"courses/{module.CourseId}/modules/{module.Id}";
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, ct);
            storagePath = await _storage.SaveFileAsync(
                Convert.ToBase64String(ms.ToArray()), fileName, folder, ct);
            fileSize = ms.Length;
        }

        var item = new ContentItem
        {
            ContentModuleId = request.ContentModuleId,
            Title = request.Title.Trim(),
            ResourceType = request.ResourceType,
            StoragePath = storagePath,
            FileSizeBytes = fileSize,
            OriginalFileName = fileName,       // ← FIX: preserve original filename
            ContentType = contentType,    // ← FIX: preserve MIME type
            UploadedByUserId = uploadedByUserId,
            CreatedBy = uploadedByUserId
        };

        await _uow.ContentItems.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ContentItemDto>(item);
    }

    public async Task<ContentItemDto> UpdateItemAsync(Guid id, UpdateContentItemRequest request, CancellationToken ct = default)
    {
        var item = await _uow.ContentItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContentItem), id);
        item.Title = request.Title.Trim();
        item.IsPublished = request.IsPublished;
        await _uow.ContentItems.UpdateAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ContentItemDto>(item);
    }

    public async Task DeleteItemAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _uow.ContentItems.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ContentItem), id);
        await DeleteItemInternalAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ReorderItemsAsync(Guid moduleId, List<Guid> orderedIds, CancellationToken ct = default)
    {
        var items = await _uow.ContentItems.FindAsync(i => i.ContentModuleId == moduleId, ct);
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == orderedIds[i]);
            if (item != null)
            {
                item.SortOrder = i;
                await _uow.ContentItems.UpdateAsync(item, ct);
            }
        }
        await _uow.SaveChangesAsync(ct);
    }

    // ── File Delivery ─────────────────────────────────────────────────────────

    public async Task<(Stream Stream, string ContentType, string FileName)> StreamFileAsync(
        Guid itemId, string requestingUserId, CancellationToken ct = default)
    {
        var item = await _uow.ContentItems.GetByIdAsync(itemId, ct)
            ?? throw new NotFoundException(nameof(ContentItem), itemId);

        if (string.IsNullOrEmpty(item.StoragePath))
            throw new DomainException("No file associated with this item.");

        var stream = await _storage.OpenReadAsync(item.StoragePath, ct);

        // FIX: use saved OriginalFileName, fall back to storage path filename
        var fileName = !string.IsNullOrEmpty(item.OriginalFileName)
            ? item.OriginalFileName
            : Path.GetFileName(item.StoragePath);

        // FIX: use saved ContentType, fall back to extension-based MIME detection
        var contentType = !string.IsNullOrEmpty(item.ContentType)
            ? item.ContentType
            : GetMimeType(fileName);

        return (stream, contentType, fileName);
    }

    // ── Course Content ────────────────────────────────────────────────────────

    public async Task<CourseContentDto> GetCourseContentAsync(Guid courseId, string? studentUserId, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(courseId, ct)
            ?? throw new NotFoundException(nameof(Course), courseId);

        var modules = await _uow.ContentModules.FindAsync(m => m.CourseId == courseId, ct);
        var orderedModules = modules.OrderBy(m => m.SortOrder).ThenBy(m => m.CreatedAt).ToList();

        Student? student = null;
        if (!string.IsNullOrEmpty(studentUserId))
            student = await _uow.Students.FirstOrDefaultAsync(s => s.UserId == studentUserId, ct);

        var moduleWithItems = new List<ContentModuleWithItemsDto>();

        foreach (var module in orderedModules)
        {
            var items = await _uow.ContentItems.FindAsync(i => i.ContentModuleId == module.Id, ct);
            var orderedItems = items.OrderBy(i => i.SortOrder).ThenBy(i => i.CreatedAt).ToList();

            var itemDtos = orderedItems.Select(item => new ContentItemSummaryDto
            {
                Id = item.Id,
                Title = item.Title,
                ResourceType = item.ResourceType,
                AccessLevel = item.AccessLevel,
                SortOrder = item.SortOrder,
                IsPublished = item.IsPublished,
                FileSizeBytes = item.FileSizeBytes,
                DurationSeconds = item.DurationSeconds,
                ExternalUrl = item.ExternalUrl,
                IsCompleted = false
            }).ToList();

            if (student is not null && orderedItems.Any())
            {
                var itemIds = orderedItems.Select(i => i.Id).ToList();
                var progressList = await _uow.StudentContentProgress.FindAsync(
                    p => p.StudentId == student.Id && itemIds.Contains(p.ContentItemId), ct);

                foreach (var itemDto in itemDtos)
                {
                    var progress = progressList.FirstOrDefault(p => p.ContentItemId == itemDto.Id);
                    if (progress is not null)
                        itemDto.IsCompleted = progress.IsCompleted;
                }
            }

            var completedCount = itemDtos.Count(i => i.IsCompleted);

            moduleWithItems.Add(new ContentModuleWithItemsDto
            {
                Id = module.Id,
                Title = module.Title,
                Description = module.Description,
                SortOrder = module.SortOrder,
                Status = module.Status,
                Items = itemDtos,
                CompletedItems = completedCount
            });
        }

        var totalItems = moduleWithItems.Sum(m => m.Items.Count);
        var totalCompleted = moduleWithItems.Sum(m => m.CompletedItems);

        return new CourseContentDto
        {
            CourseId = courseId,
            CourseName = course.Name,
            Modules = moduleWithItems,
            TotalItems = totalItems,
            CompletedItems = totalCompleted
        };
    }

    // ── Progress ──────────────────────────────────────────────────────────────

    public async Task MarkProgressAsync(MarkProgressRequest request, CancellationToken ct = default)
    {
        var existing = await _uow.StudentContentProgress.FirstOrDefaultAsync(
            p => p.ContentItemId == request.ContentItemId &&
                 p.StudentId == request.StudentId, ct);

        if (existing is null)
        {
            await _uow.StudentContentProgress.AddAsync(new StudentContentProgress
            {
                ContentItemId = request.ContentItemId,
                StudentId = request.StudentId,
                IsCompleted = request.IsCompleted,
                FirstAccessedAt = DateTime.UtcNow,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
                AccessCount = 1,
                LastPositionSeconds = request.LastPositionSeconds
            }, ct);
        }
        else
        {
            existing.IsCompleted = request.IsCompleted;
            existing.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
            existing.AccessCount += 1;
            existing.LastPositionSeconds = request.LastPositionSeconds;
            await _uow.StudentContentProgress.UpdateAsync(existing, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }

    public async Task GrantAccessAsync(GrantAccessRequest request, string grantedByUserId, CancellationToken ct = default)
        => await Task.CompletedTask;

    public async Task RevokeAccessAsync(Guid contentItemId, Guid studentId, CancellationToken ct = default)
        => await Task.CompletedTask;

    public async Task<bool> HasAccessAsync(Guid contentItemId, string userId, CancellationToken ct = default)
        => true;

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task DeleteItemInternalAsync(ContentItem item, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(item.StoragePath))
            await _storage.DeleteFileAsync(item.StoragePath, ct);
        await _uow.ContentItems.DeleteAsync(item, ct);
    }

    /// <summary>
    /// Maps a file extension to its MIME type.
    /// Used as fallback when ContentType was not saved on upload (legacy records).
    /// </summary>
    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            // Documents
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            // Images
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            // Video
            ".mp4" => "video/mp4",
            ".mkv" => "video/x-matroska",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".webm" => "video/webm",
            // Audio
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            // Archives
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            _ => "application/octet-stream"
        };
    }
}