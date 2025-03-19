using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Calendar.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<User> _users;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
            _users = context.Users;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _users
                    .AsNoTracking()
                    .OrderBy(u => u.Username)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving users", ex);
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            try
            {
                var user = await _users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }

                return user;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving user with ID {id}", ex);
            }
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            try
            {
                var user = await _users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with username '{username}' not found");
                }

                return user;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving user with username '{username}'", ex);
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                var user = await _users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with email '{email}' not found");
                }

                return user;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving user with email '{email}'", ex);
            }
        }

        public async Task<User> GetUserWithSettingsAsync(int userId)
        {
            try
            {
                var user = await _users
                    .AsNoTracking()
                    .Include(u => u.UserSettings)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                return user;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving user with ID {userId} and settings", ex);
            }
        }

        public async Task<User> AddAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.Id = 0; // 確保 ID 是由資料庫生成
                user.CreatedAt = DateTime.UtcNow;

                var entry = await _users.AddAsync(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return entry.Entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException("Error creating user", ex);
            }
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await _users.AnyAsync(u => u.Id == user.Id))
                {
                    throw new KeyNotFoundException($"User with ID {user.Id} not found");
                }

                _users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error updating user with ID {user.Id}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _users.FindAsync(id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }

                _users.Remove(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error deleting user with ID {id}", ex);
            }
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            try
            {
                return !await _users
                    .AsNoTracking()
                    .AnyAsync(u => u.Username.ToLower() == username.ToLower());
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error checking uniqueness of username '{username}'", ex);
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            try
            {
                return !await _users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error checking uniqueness of email '{email}'", ex);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error checking existence of user with ID {id}", ex);
            }
        }
    }
}