using AutoMapper;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Calendar.API.DTOs.TagDtos;

namespace Calendar.API.Mappings
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<TodoCreateDto, Todo>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
                .ForMember(dest => dest.Description, opt => 
                    opt.MapFrom(src => src.Description != null ? src.Description.Trim() : null))
                .ForMember(dest => dest.TodoTags, opt => opt.Ignore());

            CreateMap<TodoUpdateDto, Todo>()
                .ForMember(dest => dest.TodoTags, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Todo, TodoResponseDto>()
                .ForMember(dest => dest.SubTasks, opt => 
                    opt.MapFrom(src => src.SubTasks.OrderBy(st => st.CreatedAt)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => 
                    src.TodoTags.Select(tt => new TagDto
                    {
                        Id = tt.Tag.Id,
                        Name = tt.Tag.Name,
                        Color = tt.Tag.Color
                    })));

            CreateMap<Tag, TagDto>();
        }
    }
}