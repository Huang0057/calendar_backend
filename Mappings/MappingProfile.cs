using AutoMapper;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Calendar.API.Common.Enums;

namespace Calendar.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Todo, TodoResponseDto>();
            CreateMap<TodoCreateDto, Todo>();
            CreateMap<TodoUpdateDto, Todo>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => 
                    srcMember != null));
        }
    }
}