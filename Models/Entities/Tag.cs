namespace Calendar.API.Models.Entities
{
    public class Tag
    {
        public Tag()
        {
            TodoTags = new HashSet<TodoTag>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        
        public ICollection<TodoTag> TodoTags { get; set; }
    }
}