using Calendar.API.DTOs.UserSettingDtos;

namespace Calendar.API.Services.Interfaces
{
    public interface IUserSettingService
    {
        Task<IEnumerable<UserSettingResponseDto>> GetAllUserSettingsAsync(int userId);
        Task<UserSettingResponseDto> GetUserSettingByIdAsync(int id);
        Task<UserSettingResponseDto> GetUserSettingByKeyAsync(int userId, string key);
        Task<UserSettingResponseDto> CreateUserSettingAsync(int userId, UserSettingCreateDto dto);
        Task<UserSettingResponseDto> UpdateUserSettingAsync(int id, UserSettingUpdateDto dto);
        Task DeleteUserSettingAsync(int id);
    }
}