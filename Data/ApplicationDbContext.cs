using Calendar.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Todo> Todos { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TodoTag> TodoTags { get; set; }
        
        // 會員系統資料表
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // TodoTag 關聯
            modelBuilder.Entity<TodoTag>()
                .HasKey(tt => new { tt.TodoId, tt.TagId });

            modelBuilder.Entity<TodoTag>()
                .HasOne(tt => tt.Todo)
                .WithMany(t => t.TodoTags)
                .HasForeignKey(tt => tt.TodoId);

            modelBuilder.Entity<TodoTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TodoTags)
                .HasForeignKey(tt => tt.TagId);
            
            // Todo 父子關聯
            modelBuilder.Entity<Todo>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            
            // 明確且正確的 Todo-User 關聯
            modelBuilder.Entity<Todo>()
                .HasOne(t => t.User)
                .WithMany(u => u.Todos)
                .HasForeignKey(t => t.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            
            // 使用者相關索引
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
                
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            // UserSetting 索引
            modelBuilder.Entity<UserSetting>()
                .HasIndex(us => new { us.UserId, us.Key })
                .IsUnique();
            
            // RefreshToken 索引
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
        }
    }
}