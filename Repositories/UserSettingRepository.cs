using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Calendar.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Repositories
{
    public class UserSettingRepository : IUserSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<UserSetting> _userSettings;

        public UserSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _userSettings = context.UserSettings;
        }

        public async Task<IEnumerable<UserSetting>> GetAllAsync()
        {
            try
            {
                return await _userSettings
                    .AsNoTracking()
                    .OrderBy(us => us.UserId)
                    .ThenBy(us => us.Key)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving user settings", ex);
            }
        }

        public async Task<IEnumerable<UserSetting>> GetAllUserSettingsAsync(int userId)
        {
            try
            {
                return await _userSettings
                    .AsNoTracking()
                    .Where(us => us.UserId == userId)
                    .OrderBy(us => us.Key)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error retrieving settings for user ID {userId}", ex);
            }
        }

        public async Task<UserSetting> GetByIdAsync(int id)
        {
            try
            {
                var userSetting = await _userSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(us => us.Id == id);

                if (userSetting == null)
                {
                    throw new KeyNotFoundException($"User setting with ID {id} not found");
                }

                return userSetting;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving user setting with ID {id}", ex);
            }
        }

        public async Task<UserSetting> GetSettingByKeyAsync(int userId, string key)
        {
            try
            {
                var userSetting = await _userSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(us => us.UserId == userId && us.Key == key);

                if (userSetting == null)
                {
                    throw new KeyNotFoundException($"Setting with key '{key}' for user ID {userId} not found");
                }

                return userSetting;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving setting with key '{key}' for user ID {userId}", ex);
            }
        }

        public async Task<UserSetting> AddAsync(UserSetting userSetting)
        {
            if (userSetting == null)
            {
                throw new ArgumentNullException(nameof(userSetting));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                userSetting.Id = 0; // 確保 ID 是由資料庫生成

                var entry = await _userSettings.AddAsync(userSetting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return entry.Entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException("Error creating user setting", ex);
            }
        }

        public async Task UpdateAsync(UserSetting userSetting)
        {
            if (userSetting == null)
            {
                throw new ArgumentNullException(nameof(userSetting));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await _userSettings.AnyAsync(us => us.Id == userSetting.Id))
                {
                    throw new KeyNotFoundException($"User setting with ID {userSetting.Id} not found");
                }

                _userSettings.Update(userSetting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error updating user setting with ID {userSetting.Id}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userSetting = await _userSettings.FindAsync(id);
                if (userSetting == null)
                {
                    throw new KeyNotFoundException($"User setting with ID {id} not found");
                }

                _userSettings.Remove(userSetting);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error deleting user setting with ID {id}", ex);
            }
        }
    }
}