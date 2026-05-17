using TMS.Domain.Enums;

namespace TMS.Application.DTOs.StudyMaterial;

public class BookmarkDto
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public string MaterialTitle { get; set; } = string.Empty;
    public MaterialType MaterialType { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime BookmarkedAt { get; set; }
}