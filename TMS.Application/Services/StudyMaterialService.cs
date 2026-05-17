using AutoMapper;
using TMS.Application.Common;
using TMS.Application.DTOs.StudyMaterial;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces; // Kept only if you need other domain interfaces

namespace TMS.Application.Services;

public class StudyMaterialService : IStudyMaterialService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;


    // Change your field and constructor parameter to this:
    private readonly TMS.Application.Interfaces.IFileStorageService _fileStorage;

    public StudyMaterialService(
        IUnitOfWork uow,
        IMapper mapper,
        TMS.Application.Interfaces.IFileStorageService fileStorage)
    {
        _uow = uow;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    // ── Browse ────────────────────────────────────────────────────────────────
    public async Task<PagedResult<StudyMaterialSummaryDto>> GetAllAsync(
        MaterialFilterQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _uow.StudyMaterials.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: m =>
                (query.CategoryId == null || m.CategoryId == query.CategoryId) &&
                (query.Type == null || m.Type == query.Type) &&
                (query.GradeLevel == null || m.GradeLevel == query.GradeLevel) &&
                (query.AcademicYear == null || m.AcademicYear == query.AcademicYear) &&
                (query.AllowDownload == null || m.AllowDownload == query.AllowDownload) &&
                (string.IsNullOrWhiteSpace(query.Search) ||
                    m.Title.Contains(query.Search) ||
                    (m.Description != null && m.Description.Contains(query.Search)) ||
                    (m.Tags != null && m.Tags.Contains(query.Search))),
            orderBy: m => m.CreatedAt,
            descending: true,
            ct: ct);

        var dtos = _mapper.Map<List<StudyMaterialSummaryDto>>(items);
        return PagedResult<StudyMaterialSummaryDto>.Create(
            dtos, total, query.PageNumber, query.PageSize);
    }

    public async Task<StudyMaterialDto> GetByIdAsync(
        Guid id, Guid? studentId = null, CancellationToken ct = default)
    {
        var material = await _uow.StudyMaterials.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(StudyMaterial), id);

        var dto = _mapper.Map<StudyMaterialDto>(material);

        if (studentId.HasValue)
        {
            dto.IsBookmarked = await _uow.MaterialBookmarks.ExistsAsync(
                b => b.MaterialId == id && b.StudentId == studentId.Value, ct);

            var progress = await _uow.VideoProgresses.FirstOrDefaultAsync(
                v => v.MaterialId == id && v.StudentId == studentId.Value, ct);

            if (progress is not null)
            {
                dto.VideoProgressSeconds = progress.ProgressSeconds;
                dto.VideoProgressPercentage = progress.ProgressPercentage;
                dto.VideoCompleted = progress.IsCompleted;
            }
        }

        return dto;
    }

    // ── Admin / Tutor ─────────────────────────────────────────────────────────
    public async Task<StudyMaterialDto> CreateAsync(
        CreateStudyMaterialRequest request,
        string uploadedByUserId,
        CancellationToken ct = default)
    {
        var category = await _uow.MaterialCategories.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(MaterialCategory), request.CategoryId);

        // Save main file
        var fileUrl = await _fileStorage.SaveFileAsync(
            request.FileBase64,
            request.FileName,
            $"materials/{category.SubjectId}/{request.Type}",
            ct);

        // Save optional thumbnail
        string? thumbnailUrl = null;
        if (!string.IsNullOrWhiteSpace(request.ThumbnailBase64) &&
            !string.IsNullOrWhiteSpace(request.ThumbnailFileName))
        {
            thumbnailUrl = await _fileStorage.SaveFileAsync(
                request.ThumbnailBase64,
                request.ThumbnailFileName,
                "thumbnails",
                ct);
        }

        var material = new StudyMaterial
        {
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Type = request.Type,
            FileType = request.FileType,
            AccessLevel = request.AccessLevel,
            FileUrl = fileUrl,
            ThumbnailUrl = thumbnailUrl,
            FileSizeBytes = request.FileSizeBytes,
            MimeType = request.MimeType,
            AllowDownload = request.AllowDownload,
            Tags = request.Tags?.Trim(),
            AcademicYear = request.AcademicYear,
            GradeLevel = request.GradeLevel?.Trim(),
            DurationSeconds = request.DurationSeconds,
            UploadedByUserId = uploadedByUserId,
            ViewCount = 0,
            DownloadCount = 0
        };

        await _uow.StudyMaterials.AddAsync(material, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<StudyMaterialDto>(material);
    }

    public async Task<StudyMaterialDto> UpdateAsync(
        Guid id, UpdateStudyMaterialRequest request, CancellationToken ct = default)
    {
        var material = await _uow.StudyMaterials.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(StudyMaterial), id);

        // Replace file if new one provided
        if (!string.IsNullOrWhiteSpace(request.FileBase64) &&
            !string.IsNullOrWhiteSpace(request.FileName))
        {
            await _fileStorage.DeleteFileAsync(material.FileUrl, ct);

            var category = await _uow.MaterialCategories.GetByIdAsync(material.CategoryId, ct);
            material.FileUrl = await _fileStorage.SaveFileAsync(
                request.FileBase64,
                request.FileName,
                $"materials/{category?.SubjectId}/{material.Type}",
                ct);

            material.MimeType = request.MimeType ?? material.MimeType;
            material.FileSizeBytes = request.FileSizeBytes ?? material.FileSizeBytes;
        }

        // Replace thumbnail if new one provided
        if (!string.IsNullOrWhiteSpace(request.ThumbnailBase64) &&
            !string.IsNullOrWhiteSpace(request.ThumbnailFileName))
        {
            if (!string.IsNullOrWhiteSpace(material.ThumbnailUrl))
                await _fileStorage.DeleteFileAsync(material.ThumbnailUrl, ct);

            material.ThumbnailUrl = await _fileStorage.SaveFileAsync(
                request.ThumbnailBase64,
                request.ThumbnailFileName,
                "thumbnails",
                ct);
        }

        material.Title = request.Title.Trim();
        material.Description = request.Description?.Trim();
        material.AccessLevel = request.AccessLevel;
        material.AllowDownload = request.AllowDownload;
        material.Tags = request.Tags?.Trim();
        material.AcademicYear = request.AcademicYear;
        material.GradeLevel = request.GradeLevel?.Trim();
        material.DurationSeconds = request.DurationSeconds;

        await _uow.StudyMaterials.UpdateAsync(material, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<StudyMaterialDto>(material);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var material = await _uow.StudyMaterials.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(StudyMaterial), id);

        await _fileStorage.DeleteFileAsync(material.FileUrl, ct);
        if (!string.IsNullOrWhiteSpace(material.ThumbnailUrl))
            await _fileStorage.DeleteFileAsync(material.ThumbnailUrl, ct);

        await _uow.StudyMaterials.DeleteAsync(material, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Student Interactions ──────────────────────────────────────────────────
    public async Task IncrementViewCountAsync(Guid materialId, CancellationToken ct = default)
    {
        var material = await _uow.StudyMaterials.GetByIdAsync(materialId, ct)
            ?? throw new NotFoundException(nameof(StudyMaterial), materialId);

        material.ViewCount++;
        await _uow.StudyMaterials.UpdateAsync(material, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RecordDownloadAsync(
        Guid materialId, Guid studentId,
        string? ipAddress = null, CancellationToken ct = default)
    {
        var material = await _uow.StudyMaterials.GetByIdAsync(materialId, ct)
            ?? throw new NotFoundException(nameof(StudyMaterial), materialId);

        if (!material.AllowDownload)
            throw new DomainException("Downloads are not permitted for this material.");

        var download = new MaterialDownload
        {
            MaterialId = materialId,
            StudentId = studentId,
            DownloadedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        material.DownloadCount++;

        await _uow.MaterialDownloads.AddAsync(download, ct);
        await _uow.StudyMaterials.UpdateAsync(material, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Bookmarks ─────────────────────────────────────────────────────────────
    public async Task<List<BookmarkDto>> GetBookmarksAsync(Guid studentId, CancellationToken ct = default)
    {
        var bookmarks = await _uow.MaterialBookmarks.FindAsync(
            b => b.StudentId == studentId, ct);

        return _mapper.Map<List<BookmarkDto>>(
            bookmarks.OrderByDescending(b => b.BookmarkedAt).ToList());
    }

    public async Task<BookmarkDto> AddBookmarkAsync(
        Guid materialId, Guid studentId, CancellationToken ct = default)
    {
        _ = await _uow.StudyMaterials.GetByIdAsync(materialId, ct)
            ?? throw new NotFoundException(nameof(StudyMaterial), materialId);

        if (await _uow.MaterialBookmarks.ExistsAsync(
                b => b.MaterialId == materialId && b.StudentId == studentId, ct))
            throw new DomainException("Material is already bookmarked.");

        var bookmark = new MaterialBookmark
        {
            MaterialId = materialId,
            StudentId = studentId,
            BookmarkedAt = DateTime.UtcNow
        };

        await _uow.MaterialBookmarks.AddAsync(bookmark, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<BookmarkDto>(bookmark);
    }

    public async Task RemoveBookmarkAsync(
        Guid materialId, Guid studentId, CancellationToken ct = default)
    {
        var bookmark = await _uow.MaterialBookmarks.FirstOrDefaultAsync(
            b => b.MaterialId == materialId && b.StudentId == studentId, ct)
            ?? throw new NotFoundException("Bookmark", $"Material {materialId} / Student {studentId}");

        await _uow.MaterialBookmarks.DeleteAsync(bookmark, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Video Progress ────────────────────────────────────────────────────────
    public async Task<VideoProgressDto?> GetVideoProgressAsync(
        Guid materialId, Guid studentId, CancellationToken ct = default)
    {
        var progress = await _uow.VideoProgresses.FirstOrDefaultAsync(
            v => v.MaterialId == materialId && v.StudentId == studentId, ct);

        return progress is null ? null : _mapper.Map<VideoProgressDto>(progress);
    }

    public async Task<VideoProgressDto> UpsertVideoProgressAsync(
        UpdateVideoProgressRequest request, CancellationToken ct = default)
    {
        var existing = await _uow.VideoProgresses.FirstOrDefaultAsync(
            v => v.MaterialId == request.MaterialId &&
                 v.StudentId == request.StudentId, ct);

        if (existing is null)
        {
            var progress = new VideoProgress
            {
                MaterialId = request.MaterialId,
                StudentId = request.StudentId,
                ProgressSeconds = request.ProgressSeconds,
                TotalDurationSeconds = request.TotalDurationSeconds,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
                LastWatchedAt = DateTime.UtcNow
            };

            await _uow.VideoProgresses.AddAsync(progress, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<VideoProgressDto>(progress);
        }

        existing.ProgressSeconds = request.ProgressSeconds;
        existing.TotalDurationSeconds = request.TotalDurationSeconds;
        existing.LastWatchedAt = DateTime.UtcNow;

        if (request.IsCompleted && !existing.IsCompleted)
        {
            existing.IsCompleted = true;
            existing.CompletedAt = DateTime.UtcNow;
        }

        await _uow.VideoProgresses.UpdateAsync(existing, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<VideoProgressDto>(existing);
    }

    public async Task<List<VideoProgressDto>> GetContinueWatchingAsync(
        Guid studentId, CancellationToken ct = default)
    {
        var progresses = await _uow.VideoProgresses.FindAsync(
            v => v.StudentId == studentId &&
                 !v.IsCompleted &&
                 v.ProgressSeconds > 0, ct);

        var ordered = progresses
            .OrderByDescending(v => v.LastWatchedAt)
            .Take(10)
            .ToList();

        return _mapper.Map<List<VideoProgressDto>>(ordered);
    }
}