// Models/Entities/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calendar.API.Models.Entities
{
    public class User
    {
        public User()
        {
            Todos = new HashSet<Todo>();
            RefreshTokens = new HashSet<RefreshToken>();
            UserSettings = new HashSet<UserSetting>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<Todo> Todos { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<UserSetting> UserSettings { get; set; }
    }
}