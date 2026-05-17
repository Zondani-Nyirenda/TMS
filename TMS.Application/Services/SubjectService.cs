using AutoMapper;
using TMS.Application.Common;
using TMS.Application.DTOs.StudyMaterial;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SubjectService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    // ── Subjects ──────────────────────────────────────────────────────────────

    public async Task<List<SubjectSummaryDto>> GetAllAsync(
        bool activeOnly = true, CancellationToken ct = default)
    {
        var subjects = activeOnly
            ? await _uow.Subjects.FindAsync(s => s.IsActive, ct)
            : await _uow.Subjects.GetAllAsync(ct);

        var ordered = subjects.OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToList();
        var dtos = _mapper.Map<List<SubjectSummaryDto>>(ordered);

        // Enrich with counts
        for (int i = 0; i < ordered.Count; i++)
        {
            var categories = await _uow.MaterialCategories
                .FindAsync(c => c.SubjectId == ordered[i].Id, ct);

            var categoryIds = categories.Select(c => c.Id).ToList();

            var materialCount = categoryIds.Count == 0 ? 0 :
                await _uow.StudyMaterials.CountAsync(
                    m => categoryIds.Contains(m.CategoryId), ct);

            dtos[i].CategoryCount = categories.Count;
            dtos[i].MaterialCount = materialCount;
        }

        return dtos;
    }

    public async Task<SubjectDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Subject), id);

        var dto = _mapper.Map<SubjectDto>(subject);

        var categories = await _uow.MaterialCategories
            .FindAsync(c => c.SubjectId == id, ct);

        var categoryIds = categories.Select(c => c.Id).ToList();

        var materialCount = categoryIds.Count == 0 ? 0 :
            await _uow.StudyMaterials.CountAsync(
                m => categoryIds.Contains(m.CategoryId), ct);

        dto.CategoryCount = categories.Count;
        dto.MaterialCount = materialCount;

        return dto;
    }

    public async Task<SubjectDto> CreateAsync(
        CreateSubjectRequest request, CancellationToken ct = default)
    {
        // Duplicate name check
        if (await _uow.Subjects.ExistsAsync(s => s.Name == request.Name.Trim(), ct))
            throw new DomainException($"A subject named '{request.Name}' already exists.");

        var subject = new Subject
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IconEmoji = request.IconEmoji?.Trim(),
            ColorHex = request.ColorHex?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        await _uow.Subjects.AddAsync(subject, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto> UpdateAsync(
        Guid id, UpdateSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Subject), id);

        // Duplicate name check (exclude self)
        if (subject.Name != request.Name.Trim() &&
            await _uow.Subjects.ExistsAsync(
                s => s.Name == request.Name.Trim() && s.Id != id, ct))
            throw new DomainException($"A subject named '{request.Name}' already exists.");

        subject.Name = request.Name.Trim();
        subject.Description = request.Description?.Trim();
        subject.IconEmoji = request.IconEmoji?.Trim();
        subject.ColorHex = request.ColorHex?.Trim();
        subject.SortOrder = request.SortOrder;
        subject.IsActive = request.IsActive;

        await _uow.Subjects.UpdateAsync(subject, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<SubjectDto>(subject);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Subject), id);

        await _uow.Subjects.DeleteAsync(subject, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Categories ────────────────────────────────────────────────────────────

    public async Task<List<MaterialCategorySummaryDto>> GetCategoriesAsync(
        Guid subjectId, CancellationToken ct = default)
    {
        _ = await _uow.Subjects.GetByIdAsync(subjectId, ct)
            ?? throw new NotFoundException(nameof(Subject), subjectId);

        var categories = await _uow.MaterialCategories
            .FindAsync(c => c.SubjectId == subjectId, ct);

        var ordered = categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        var dtos = _mapper.Map<List<MaterialCategorySummaryDto>>(ordered);

        // Enrich with material counts
        for (int i = 0; i < ordered.Count; i++)
        {
            dtos[i].MaterialCount = await _uow.StudyMaterials.CountAsync(
                m => m.CategoryId == ordered[i].Id, ct);
        }

        return dtos;
    }

    public async Task<MaterialCategoryDto> GetCategoryByIdAsync(
        Guid categoryId, CancellationToken ct = default)
    {
        var category = await _uow.MaterialCategories.GetByIdAsync(categoryId, ct)
            ?? throw new NotFoundException(nameof(MaterialCategory), categoryId);

        var dto = _mapper.Map<MaterialCategoryDto>(category);

        dto.MaterialCount = await _uow.StudyMaterials.CountAsync(
            m => m.CategoryId == categoryId, ct);

        return dto;
    }

    public async Task<MaterialCategoryDto> CreateCategoryAsync(
        CreateMaterialCategoryRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Subjects.GetByIdAsync(request.SubjectId, ct)
            ?? throw new NotFoundException(nameof(Subject), request.SubjectId);

        // Duplicate name within same subject
        if (await _uow.MaterialCategories.ExistsAsync(
                c => c.SubjectId == request.SubjectId &&
                     c.Name == request.Name.Trim(), ct))
            throw new DomainException(
                $"Category '{request.Name}' already exists in this subject.");

        var category = new MaterialCategory
        {
            SubjectId = request.SubjectId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IconEmoji = request.IconEmoji?.Trim(),
            SortOrder = request.SortOrder
        };

        await _uow.MaterialCategories.AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<MaterialCategoryDto>(category);
    }

    public async Task<MaterialCategoryDto> UpdateCategoryAsync(
        Guid id, UpdateMaterialCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _uow.MaterialCategories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(MaterialCategory), id);

        // Duplicate name check within same subject (exclude self)
        if (category.Name != request.Name.Trim() &&
            await _uow.MaterialCategories.ExistsAsync(
                c => c.SubjectId == category.SubjectId &&
                     c.Name == request.Name.Trim() &&
                     c.Id != id, ct))
            throw new DomainException(
                $"Category '{request.Name}' already exists in this subject.");

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.IconEmoji = request.IconEmoji?.Trim();
        category.SortOrder = request.SortOrder;

        await _uow.MaterialCategories.UpdateAsync(category, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<MaterialCategoryDto>(category);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _uow.MaterialCategories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(MaterialCategory), id);

        await _uow.MaterialCategories.DeleteAsync(category, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Class Mappings ────────────────────────────────────────────────────────

    public async Task AssignToClassAsync(
        Guid subjectId, Guid classId, CancellationToken ct = default)
    {
        _ = await _uow.Subjects.GetByIdAsync(subjectId, ct)
            ?? throw new NotFoundException(nameof(Subject), subjectId);

        _ = await _uow.Classes.GetByIdAsync(classId, ct)
            ?? throw new NotFoundException(nameof(Class), classId);

        if (await _uow.SubjectClassMappings.ExistsAsync(
                m => m.SubjectId == subjectId && m.ClassId == classId, ct))
            throw new DomainException("Subject is already assigned to this class.");

        await _uow.SubjectClassMappings.AddAsync(new SubjectClassMapping
        {
            SubjectId = subjectId,
            ClassId = classId
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task RemoveFromClassAsync(
        Guid subjectId, Guid classId, CancellationToken ct = default)
    {
        var mapping = await _uow.SubjectClassMappings.FirstOrDefaultAsync(
            m => m.SubjectId == subjectId && m.ClassId == classId, ct)
            ?? throw new NotFoundException("SubjectClassMapping",
                $"Subject {subjectId} / Class {classId}");

        await _uow.SubjectClassMappings.DeleteAsync(mapping, ct);
        await _uow.SaveChangesAsync(ct);
    }
}