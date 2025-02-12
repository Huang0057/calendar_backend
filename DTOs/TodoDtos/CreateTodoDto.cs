using System.ComponentModel.DataAnnotations;

namespace Calendar.API.DTOs.TodoDtos
{
    public class TodoCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? DueDate { get; set; }
    }
}