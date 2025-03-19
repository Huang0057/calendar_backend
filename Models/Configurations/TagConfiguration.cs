// Models/Configurations/TagConfiguration.cs
using Calendar.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.API.Models.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");          
                      
            // 標籤名稱應該是唯一的，或者至少在同一用戶下是唯一的
            builder.HasIndex(t => t.Name);
            
            // 限制欄位長度，避免過長的標籤名稱和顏色值
            builder.Property(t => t.Name)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(t => t.Color)
                .HasMaxLength(20)
                .IsRequired();
                
            // 配置與 TodoTag 的關聯
            builder.HasMany(t => t.TodoTags)
                .WithOne(tt => tt.Tag)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}