using Calendar.API.Common.Enums;

namespace Calendar.API.Models.Entities
{
    public class Todo
    {
        public Todo()
        {
            SubTasks = new HashSet<Todo>();
            TodoTags = new HashSet<TodoTag>();
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public int? ParentId { get; set; }
        
        // Navigation properties
        public Todo? Parent { get; set; }
        public ICollection<Todo> SubTasks { get; set; }
        public ICollection<TodoTag> TodoTags { get; set; }
    }
}

