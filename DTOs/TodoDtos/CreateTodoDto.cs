using System.ComponentModel.DataAnnotations;
using Calendar.API.Common.Enums ;
namespace Calendar.API.DTOs.TodoDtos
{
    public class TodoCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public Priority Priority { get; set; } = Priority.Medium;
        
        public DateTime? DueDate { get; set; }
        
        public List<int> TagIds { get; set; } = new List<int>();
    }
    public class SubTaskCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
    }
}