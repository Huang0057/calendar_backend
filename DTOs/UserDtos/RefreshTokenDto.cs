using System.ComponentModel.DataAnnotations;

namespace Calendar.API.DTOs.UserDtos
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}