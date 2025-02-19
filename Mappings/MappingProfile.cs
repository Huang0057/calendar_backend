using AutoMapper;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Calendar.API.Common.Enums;

namespace Calendar.API.Mappings
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<TodoCreateDto, Todo>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Trim()))
                .ForMember(dest => dest.Description, opt => 
                    opt.MapFrom(src => src.Description != null ? src.Description.Trim() : null));

            CreateMap<TodoUpdateDto, Todo>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Todo, TodoResponseDto>()
                .ForMember(dest => dest.SubTasks, opt => 
                    opt.MapFrom(src => src.SubTasks.OrderBy(st => st.CreatedAt)));
        }
    }
}