using Calendar.API.Models.Entities;

namespace Calendar.API.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<IEnumerable<RefreshToken>> GetAllAsync();
        Task<RefreshToken> GetByIdAsync(int id);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetAllValidTokensByUserIdAsync(int userId);
        Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync();
        Task<RefreshToken> AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteAsync(int id);
        Task RevokeAllUserTokensAsync(int userId);
    }
}