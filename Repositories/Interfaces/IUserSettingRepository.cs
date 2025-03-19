using Calendar.API.Models.Entities;

namespace Calendar.API.Repositories
{
    public interface IUserSettingRepository
    {
        Task<IEnumerable<UserSetting>> GetAllAsync();
        Task<IEnumerable<UserSetting>> GetAllUserSettingsAsync(int userId);
        Task<UserSetting> GetByIdAsync(int id);
        Task<UserSetting> GetSettingByKeyAsync(int userId, string key);
        Task<UserSetting> AddAsync(UserSetting userSetting);
        Task UpdateAsync(UserSetting userSetting);
        Task DeleteAsync(int id);
    }
}