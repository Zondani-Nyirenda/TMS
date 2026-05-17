using AutoMapper;
using TMS.Application.DTOs.StudyMaterial;
using TMS.Domain.Entities;

namespace TMS.Application.Mappings;

public class StudyMaterialMappingProfile : Profile
{
    public StudyMaterialMappingProfile()
    {
        // Subject
        CreateMap<Subject, SubjectDto>()
            .ForMember(d => d.CategoryCount, o => o.Ignore())
            .ForMember(d => d.MaterialCount, o => o.Ignore());

        CreateMap<Subject, SubjectSummaryDto>()
            .ForMember(d => d.CategoryCount, o => o.Ignore())
            .ForMember(d => d.MaterialCount, o => o.Ignore());

        // MaterialCategory
        CreateMap<MaterialCategory, MaterialCategoryDto>()
            .ForMember(d => d.SubjectName,
                o => o.MapFrom(s => s.Subject != null ? s.Subject.Name : string.Empty))
            .ForMember(d => d.MaterialCount, o => o.Ignore());

        CreateMap<MaterialCategory, MaterialCategorySummaryDto>()
            .ForMember(d => d.MaterialCount, o => o.Ignore());

        // StudyMaterial
        CreateMap<StudyMaterial, StudyMaterialDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.SubjectName,
                o => o.MapFrom(s => s.Category != null && s.Category.Subject != null
                    ? s.Category.Subject.Name : string.Empty))
            .ForMember(d => d.UploadedByName,
                o => o.MapFrom(s => s.UploadedByUser != null
                    ? $"{s.UploadedByUser.FirstName} {s.UploadedByUser.LastName}"
                    : string.Empty))
            .ForMember(d => d.IsBookmarked, o => o.Ignore())
            .ForMember(d => d.VideoProgressSeconds, o => o.Ignore())
            .ForMember(d => d.VideoProgressPercentage, o => o.Ignore())
            .ForMember(d => d.VideoCompleted, o => o.Ignore());

        CreateMap<StudyMaterial, StudyMaterialSummaryDto>()
            .ForMember(d => d.IsBookmarked, o => o.Ignore())
            .ForMember(d => d.VideoProgressPercentage, o => o.Ignore());

        // Bookmark
        CreateMap<MaterialBookmark, BookmarkDto>()
            .ForMember(d => d.MaterialTitle,
                o => o.MapFrom(s => s.Material != null ? s.Material.Title : string.Empty))
            .ForMember(d => d.MaterialType,
                o => o.MapFrom(s => s.Material != null ? s.Material.Type : default))
            .ForMember(d => d.ThumbnailUrl,
                o => o.MapFrom(s => s.Material != null ? s.Material.ThumbnailUrl : null))
            .ForMember(d => d.SubjectName,
                o => o.MapFrom(s => s.Material != null &&
                                    s.Material.Category != null &&
                                    s.Material.Category.Subject != null
                    ? s.Material.Category.Subject.Name : string.Empty))
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Material != null && s.Material.Category != null
                    ? s.Material.Category.Name : string.Empty));

        // VideoProgress
        CreateMap<VideoProgress, VideoProgressDto>()
            .ForMember(d => d.MaterialTitle,
                o => o.MapFrom(s => s.Material != null ? s.Material.Title : string.Empty))
            .ForMember(d => d.ThumbnailUrl,
                o => o.MapFrom(s => s.Material != null ? s.Material.ThumbnailUrl : null))
            .ForMember(d => d.SubjectName,
                o => o.MapFrom(s => s.Material != null &&
                                    s.Material.Category != null &&
                                    s.Material.Category.Subject != null
                    ? s.Material.Category.Subject.Name : string.Empty))
            .ForMember(d => d.ProgressPercentage,
                o => o.MapFrom(s => s.ProgressPercentage));
    }
}