using AutoMapper;
using Calendar.API.DTOs.UserSettingDtos;
using Calendar.API.Models.Entities;
using Calendar.API.Repositories;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;

namespace Calendar.API.Services
{
    public class UserSettingService : IUserSettingService
    {
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserSettingService(
            IUserSettingRepository userSettingRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userSettingRepository = userSettingRepository ?? throw new ArgumentNullException(nameof(userSettingRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<UserSettingResponseDto>> GetAllUserSettingsAsync(int userId)
        {
            try
            {
                // 檢查用戶是否存在
                await CheckUserExistsAsync(userId);

                var settings = await _userSettingRepository.GetAllUserSettingsAsync(userId);
                return _mapper.Map<IEnumerable<UserSettingResponseDto>>(settings);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve settings for user ID {userId}", ex);
            }
        }

        public async Task<UserSettingResponseDto> GetUserSettingByIdAsync(int id)
        {
            try
            {
                var setting = await _userSettingRepository.GetByIdAsync(id);
                return _mapper.Map<UserSettingResponseDto>(setting);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User setting with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve user setting with ID {id}", ex);
            }
        }

        public async Task<UserSettingResponseDto> GetUserSettingByKeyAsync(int userId, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Setting key cannot be empty or whitespace", nameof(key));
            }

            try
            {
                // 檢查用戶是否存在
                await CheckUserExistsAsync(userId);

                var setting = await _userSettingRepository.GetSettingByKeyAsync(userId, key);
                return _mapper.Map<UserSettingResponseDto>(setting);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"Setting with key '{key}' for user ID {userId} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve setting with key '{key}' for user ID {userId}", ex);
            }
        }

        public async Task<UserSettingResponseDto> CreateUserSettingAsync(int userId, UserSettingCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Key))
            {
                throw new ArgumentException("Setting key cannot be empty or whitespace");
            }

            try
            {
                // 檢查用戶是否存在
                await CheckUserExistsAsync(userId);

                // 檢查相同鍵值的設定是否已存在
                var existingSettings = await _userSettingRepository.GetAllUserSettingsAsync(userId);
                if (existingSettings.Any(s => s.Key.Equals(dto.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new DuplicateEntityException($"Setting with key '{dto.Key}' already exists for user ID {userId}");
                }

                var userSetting = new UserSetting
                {
                    UserId = userId,
                    Key = dto.Key,
                    Value = dto.Value
                };

                var createdSetting = await _userSettingRepository.AddAsync(userSetting);
                return _mapper.Map<UserSettingResponseDto>(createdSetting);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to create setting for user ID {userId}", ex);
            }
        }

        public async Task<UserSettingResponseDto> UpdateUserSettingAsync(int id, UserSettingUpdateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                var userSetting = await _userSettingRepository.GetByIdAsync(id);
                
                // 更新值
                userSetting.Value = dto.Value;

                await _userSettingRepository.UpdateAsync(userSetting);
                return _mapper.Map<UserSettingResponseDto>(userSetting);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User setting with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to update user setting with ID {id}", ex);
            }
        }

        public async Task DeleteUserSettingAsync(int id)
        {
            try
            {
                await _userSettingRepository.GetByIdAsync(id); // 檢查設定是否存在
                await _userSettingRepository.DeleteAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"User setting with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to delete user setting with ID {id}", ex);
            }
        }

        private async Task CheckUserExistsAsync(int userId)
        {
            if (!await _userRepository.ExistsAsync(userId))
            {
                throw new EntityNotFoundException($"User with ID {userId} not found");
            }
        }
    }
}