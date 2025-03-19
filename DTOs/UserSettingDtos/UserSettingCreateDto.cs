using System.ComponentModel.DataAnnotations;

namespace Calendar.API.DTOs.UserSettingDtos
{
    public class UserSettingCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }
    }
}
