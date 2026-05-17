namespace TMS.Application.DTOs.StudyMaterial;

public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int CategoryCount { get; set; }
    public int MaterialCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubjectSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconEmoji { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int CategoryCount { get; set; }
    public int MaterialCount { get; set; }
}

public class CreateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconEmoji { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}