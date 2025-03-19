using Calendar.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.API.Models.Configurations
{
    public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
    {
        public void Configure(EntityTypeBuilder<UserSetting> builder)
        {
            builder.ToTable("UserSettings");
            
            // 確保每個使用者的設定鍵是唯一的
            builder.HasIndex(us => new { us.UserId, us.Key }).IsUnique();
        }
    }
}
