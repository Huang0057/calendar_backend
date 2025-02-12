using Calendar.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Todo> Todos {get; set;}
        public DbSet<Tag> Tags {get; set;}
        public DbSet<TodoTag> TodoTags {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<TodoTag>()
                .HasKey(tt => new { tt.TodoId, tt.TagId});

            modelBuilder.Entity<TodoTag>()
                .HasOne(tt => tt.Todo)
                .WithMany(t => t.TodoTags)
                .HasForeignKey(tt => tt.TodoId);

            modelBuilder.Entity<TodoTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TodoTags)
                .HasForeignKey(tt => tt.TagId);
            
            modelBuilder.Entity<Todo>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.ParentId);

        }
    }
}