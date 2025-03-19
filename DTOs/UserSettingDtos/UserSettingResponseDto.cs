namespace Calendar.API.DTOs.UserSettingDtos
{
    public class UserSettingResponseDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public int UserId { get; set; }
    }
}