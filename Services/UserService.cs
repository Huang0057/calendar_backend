using AutoMapper;
using Calendar.API.DTOs.UserDtos;
using Calendar.API.Models.Entities;
using Calendar.API.Repositories;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Calendar.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IMapper mapper,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<UserResponseDto>>(users);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException("Failed to retrieve users", ex);
            }
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                return _mapper.Map<UserResponseDto>(user);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve user with ID {id}", ex);
            }
        }

        public async Task<UserResponseDto> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty or whitespace", nameof(username));
            }

            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                return _mapper.Map<UserResponseDto>(user);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User with username '{username}' was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve user with username '{username}'", ex);
            }
        }

        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            ValidateUserDto(dto);

            try
            {
                // 檢查用戶名和電子郵件是否唯一
                if (!await IsUsernameUniqueAsync(dto.Username))
                {
                    throw new DuplicateEntityException($"Username '{dto.Username}' is already taken");
                }

                if (!await IsEmailUniqueAsync(dto.Email))
                {
                    throw new DuplicateEntityException($"Email '{dto.Email}' is already registered");
                }

                var user = _mapper.Map<User>(dto);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                
                // 密碼雜湊處理
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

                var createdUser = await _userRepository.AddAsync(user);
                return _mapper.Map<UserResponseDto>(createdUser);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException("Failed to create user", ex);
            }
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                // 檢查用戶名和電子郵件是否唯一
                if (dto.Username != null && dto.Username != user.Username && !await IsUsernameUniqueAsync(dto.Username, id))
                {
                    throw new DuplicateEntityException($"Username '{dto.Username}' is already taken");
                }

                if (dto.Email != null && dto.Email != user.Email && !await IsEmailUniqueAsync(dto.Email, id))
                {
                    throw new DuplicateEntityException($"Email '{dto.Email}' is already registered");
                }

                // 更新屬性
                if (!string.IsNullOrWhiteSpace(dto.Username))
                    user.Username = dto.Username;
                
                if (!string.IsNullOrWhiteSpace(dto.Email))
                    user.Email = dto.Email;
                
                if (dto.FirstName != null)
                    user.FirstName = dto.FirstName;
                
                if (dto.LastName != null)
                    user.LastName = dto.LastName;

                await _userRepository.UpdateAsync(user);
                return _mapper.Map<UserResponseDto>(user);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to update user with ID {id}", ex);
            }
        }

        public async Task ChangePasswordAsync(int id, UserChangePasswordDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                // 驗證當前密碼
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    throw new BusinessValidationException("Current password is incorrect");
                }

                // 更新密碼
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);

                await _userRepository.UpdateAsync(user);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to change password for user with ID {id}", ex);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                await _userRepository.GetByIdAsync(id); // 檢查用戶是否存在
                await _userRepository.DeleteAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to delete user with ID {id}", ex);
            }
        }

        public async Task<TokenResponseDto> LoginAsync(UserLoginDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                // 嘗試通過用戶名查找用戶
                var user = await _userRepository.GetByUsernameAsync(dto.Username);

                // 驗證密碼
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    throw new AuthenticationException("Invalid username or password");
                }

                // 更新最後登錄時間
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // 創建 JWT token
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // 儲存刷新令牌
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false,
                    IsRevoked = false
                };

                await _refreshTokenRepository.AddAsync(refreshTokenEntity);

                return new TokenResponseDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "15")),
                    User = _mapper.Map<UserResponseDto>(user)
                };
            }
            catch (KeyNotFoundException)
            {
                throw new AuthenticationException("Invalid username or password");
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException("Login failed due to a system error", ex);
            }
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.Token);

                // 檢查令牌是否已過期、已使用或已撤銷
                if (refreshToken.ExpiryDate <= DateTime.UtcNow || refreshToken.IsUsed || refreshToken.IsRevoked)
                {
                    throw new AuthenticationException("Invalid or expired refresh token");
                }

                // 標記當前令牌為已使用
                refreshToken.IsUsed = true;
                await _refreshTokenRepository.UpdateAsync(refreshToken);

                // 獲取用戶
                var user = await _userRepository.GetByIdAsync(refreshToken.UserId);

                // 創建新的訪問令牌和刷新令牌
                var token = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // 儲存新的刷新令牌
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false,
                    IsRevoked = false
                };

                await _refreshTokenRepository.AddAsync(refreshTokenEntity);

                return new TokenResponseDto
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "15")),
                    User = _mapper.Map<UserResponseDto>(user)
                };
            }
            catch (KeyNotFoundException)
            {
                throw new AuthenticationException("Invalid refresh token");
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException("Token refresh failed due to a system error", ex);
            }
        }

        public async Task LogoutAsync(int userId)
        {
            try
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to logout user with ID {userId}", ex);
            }
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty or whitespace", nameof(username));
            }

            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                
                // 如果找不到用戶，表示用戶名不存在
                if (user == null)
                {
                    return true;
                }

                // 如果提供了排除ID，且找到的用戶ID與排除ID相同，表示是同一個用戶
                if (excludeId.HasValue && user.Id == excludeId.Value)
                {
                    return true;
                }

                return false;
            }
            catch (KeyNotFoundException)
            {
                return true;
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to check username uniqueness: {username}", ex);
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty or whitespace", nameof(email));
            }

            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                
                // 如果找不到用戶，表示電子郵件不存在
                if (user == null)
                {
                    return true;
                }

                // 如果提供了排除ID，且找到的用戶ID與排除ID相同，表示是同一個用戶
                if (excludeId.HasValue && user.Id == excludeId.Value)
                {
                    return true;
                }

                return false;
            }
            catch (KeyNotFoundException)
            {
                return true;
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to check email uniqueness: {email}", ex);
            }
        }

        #region Private Helper Methods

        private void ValidateUserDto(UserCreateDto dto)
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                validationErrors.Add("Username cannot be empty");
            }
            else if (dto.Username.Length < 3 || dto.Username.Length > 50)
            {
                validationErrors.Add("Username must be between 3 and 50 characters");
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                validationErrors.Add("Email cannot be empty");
            }
            else if (!IsValidEmail(dto.Email))
            {
                validationErrors.Add("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                validationErrors.Add("Password cannot be empty");
            }
            else if (dto.Password.Length < 6)
            {
                validationErrors.Add("Password must be at least 6 characters");
            }

            if (validationErrors.Any())
            {
                throw new BusinessValidationException(string.Join(Environment.NewLine, validationErrors));
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured");
            var issuer = _configuration["Jwt:Issuer"] ?? "DefaultIssuer";
            var audience = _configuration["Jwt:Audience"] ?? "DefaultAudience";
            var expiryInMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "15");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(user.FirstName))
                claims.Add(new Claim("firstName", user.FirstName));
            
            if (!string.IsNullOrEmpty(user.LastName))
                claims.Add(new Claim("lastName", user.LastName));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion
    }
}
