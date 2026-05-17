namespace TMS.Application.DTOs.StudyMaterial;

public class MaterialCategoryDto
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public int SortOrder { get; set; }
    public int MaterialCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MaterialCategorySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconEmoji { get; set; }
    public int SortOrder { get; set; }
    public int MaterialCount { get; set; }
}

public class CreateMaterialCategoryRequest
{
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public int SortOrder { get; set; } = 0;
}

public class UpdateMaterialCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public int SortOrder { get; set; }
}