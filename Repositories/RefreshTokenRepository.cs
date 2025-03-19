using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Calendar.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<RefreshToken> _refreshTokens;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
            _refreshTokens = context.RefreshTokens;
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            try
            {
                return await _refreshTokens
                    .AsNoTracking()
                    .OrderByDescending(rt => rt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving refresh tokens", ex);
            }
        }

        public async Task<RefreshToken> GetByIdAsync(int id)
        {
            try
            {
                var refreshToken = await _refreshTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(rt => rt.Id == id);

                if (refreshToken == null)
                {
                    throw new KeyNotFoundException($"Refresh token with ID {id} not found");
                }

                return refreshToken;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving refresh token with ID {id}", ex);
            }
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            try
            {
                var refreshToken = await _refreshTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(rt => rt.Token == token);

                if (refreshToken == null)
                {
                    throw new KeyNotFoundException($"Refresh token '{token}' not found");
                }

                return refreshToken;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving refresh token '{token}'", ex);
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetAllValidTokensByUserIdAsync(int userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _refreshTokens
                    .AsNoTracking()
                    .Where(rt => rt.UserId == userId && rt.ExpiryDate > now && !rt.IsUsed && !rt.IsRevoked)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error retrieving valid tokens for user ID {userId}", ex);
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _refreshTokens
                    .AsNoTracking()
                    .Where(rt => rt.ExpiryDate <= now)
                    .OrderByDescending(rt => rt.ExpiryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving expired refresh tokens", ex);
            }
        }

        public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                refreshToken.Id = 0; // 確保 ID 是由資料庫生成
                refreshToken.CreatedAt = DateTime.UtcNow;
                refreshToken.IsUsed = false;
                refreshToken.IsRevoked = false;

                var entry = await _refreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return entry.Entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException("Error creating refresh token", ex);
            }
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await _refreshTokens.AnyAsync(rt => rt.Id == refreshToken.Id))
                {
                    throw new KeyNotFoundException($"Refresh token with ID {refreshToken.Id} not found");
                }

                _refreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error updating refresh token with ID {refreshToken.Id}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var refreshToken = await _refreshTokens.FindAsync(id);
                if (refreshToken == null)
                {
                    throw new KeyNotFoundException($"Refresh token with ID {id} not found");
                }

                _refreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error deleting refresh token with ID {id}", ex);
            }
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tokens = await _refreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error revoking all tokens for user ID {userId}", ex);
            }
        }
    }
}