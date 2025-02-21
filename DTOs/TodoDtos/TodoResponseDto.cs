using Calendar.API.Common.Enums;
using Calendar.API.DTOs.TagDtos;

namespace Calendar.API.DTOs.TodoDtos
{
    public class TodoResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public int? ParentId { get; set; }
        public List<TagDto> Tags { get; set; } = new();
        public List<TodoResponseDto> SubTasks { get; set; } = new();
    }
}