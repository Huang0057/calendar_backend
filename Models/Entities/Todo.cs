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
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public int? ParentId { get; set; }
        
        // 新增: 使用者關聯
        public int UserId { get; set; }
        
        // Navigation properties
        public Todo? Parent { get; set; }
        public ICollection<Todo> SubTasks { get; set; }
        public ICollection<TodoTag> TodoTags { get; set; }
        public User User { get; set; } = null!;
    }
}

