using Calendar.API.Models.Entities;

namespace Calendar.API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetUserWithSettingsAsync(int userId);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> ExistsAsync(int id);
    }
}
