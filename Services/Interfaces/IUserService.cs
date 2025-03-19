using Calendar.API.DTOs.UserDtos;
using Calendar.API.Models.Entities;

namespace Calendar.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> GetUserByIdAsync(int id);
        Task<UserResponseDto> GetUserByUsernameAsync(string username);
        Task<UserResponseDto> CreateUserAsync(UserCreateDto dto);
        Task<UserResponseDto> UpdateUserAsync(int id, UserUpdateDto dto);
        Task ChangePasswordAsync(int id, UserChangePasswordDto dto);
        Task DeleteUserAsync(int id);
        Task<TokenResponseDto> LoginAsync(UserLoginDto dto);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task LogoutAsync(int userId);
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
    }
}