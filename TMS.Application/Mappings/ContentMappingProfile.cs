using AutoMapper;
using TMS.Application.DTOs.Content;
using TMS.Domain.Entities;

namespace TMS.Application.Mappings;

public class ContentMappingProfile : Profile
{
    public ContentMappingProfile()
    {
        // ContentModule → ContentModuleDto
        CreateMap<ContentModule, ContentModuleDto>()
            .ForMember(d => d.CourseName, o => o.MapFrom(s =>
                s.Course != null ? s.Course.Name : string.Empty))
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));

        // ContentModule → ContentModuleWithItemsDto (used in CourseContentDto)
        CreateMap<ContentModule, ContentModuleWithItemsDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.CompletedItems, o => o.Ignore());  // enriched in service

        // ContentItem → ContentItemSummaryDto
        CreateMap<ContentItem, ContentItemSummaryDto>()
            .ForMember(d => d.IsCompleted, o => o.Ignore());     // enriched per-student

        // ContentItem → ContentItemDto
        CreateMap<ContentItem, ContentItemDto>()
            .ForMember(d => d.ModuleTitle, o => o.MapFrom(s =>
                s.ContentModule != null ? s.ContentModule.Title : string.Empty))
            .ForMember(d => d.CourseId, o => o.MapFrom(s =>
                s.ContentModule != null ? s.ContentModule.CourseId : Guid.Empty))
            .ForMember(d => d.DownloadUrl, o => o.Ignore())  // set in service
            .ForMember(d => d.IsCompleted, o => o.Ignore())  // enriched
            .ForMember(d => d.LastPositionSeconds, o => o.Ignore()); // enriched
    }
}