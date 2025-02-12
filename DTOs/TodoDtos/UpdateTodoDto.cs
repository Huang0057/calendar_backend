using System.ComponentModel.DataAnnotations;
using Calendar.API.Common.Enums ;

namespace Calendar.API.DTOs.TodoDtos
{
    public class TodoUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string? Title{get; set;}                
        public string? Description { get; set; }
        
        public Priority Priority { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public List<int>? TagIds { get; set; }
    }
    public class SubTaskUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}