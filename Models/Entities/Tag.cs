using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calendar.API.Models.Entities
{
    public class Tag
    {
        public Tag()
        {
            TodoTags = new HashSet<TodoTag>();
        }

        [Key]  // 標記這是主鍵
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // 設定為自動生成的識別欄位
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        
        public ICollection<TodoTag> TodoTags { get; set; }
    }
}