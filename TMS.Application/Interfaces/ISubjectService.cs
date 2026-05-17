using TMS.Application.Common;
using TMS.Application.DTOs.StudyMaterial;

namespace TMS.Application.Interfaces;

public interface ISubjectService
{
    Task<List<SubjectSummaryDto>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<SubjectDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default);
    Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // Category management
    Task<List<MaterialCategorySummaryDto>> GetCategoriesAsync(Guid subjectId, CancellationToken ct = default);
    Task<MaterialCategoryDto> GetCategoryByIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<MaterialCategoryDto> CreateCategoryAsync(CreateMaterialCategoryRequest request, CancellationToken ct = default);
    Task<MaterialCategoryDto> UpdateCategoryAsync(Guid id, UpdateMaterialCategoryRequest request, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    // Class mapping
    Task AssignToClassAsync(Guid subjectId, Guid classId, CancellationToken ct = default);
    Task RemoveFromClassAsync(Guid subjectId, Guid classId, CancellationToken ct = default);
}