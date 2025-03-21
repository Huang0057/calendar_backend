using Microsoft.AspNetCore.Mvc;
using Calendar.API.DTOs.UserSettingDtos;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace Calendar.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/settings")]
    [Authorize]
    public class UserSettingController : ControllerBase
    {
        private readonly IUserSettingService _userSettingService;
        private readonly ILogger<UserSettingController> _logger;

        public UserSettingController(IUserSettingService userSettingService, ILogger<UserSettingController> logger)
        {
            _userSettingService = userSettingService ?? throw new ArgumentNullException(nameof(userSettingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSettingResponseDto>>> GetAllUserSettings(int userId)
        {
            try
            {
                // 驗證當前用戶只能查看自己的設定，或者是管理員
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");
                
                if (currentUserId != userId && !isAdmin)
                {
                    return Forbid("You can only access your own settings");
                }

                var settings = await _userSettingService.GetAllUserSettingsAsync(userId);
                return Ok(settings);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for settings retrieval with ID: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving settings for user ID: {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving user settings");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserSettingResponseDto>> GetUserSettingById(int userId, int id)
        {
            try
            {
                // TODO: 驗證當前用戶只能查看自己的設定，或者是管理員

                var setting = await _userSettingService.GetUserSettingByIdAsync(id);
                
                // 確保設定確實屬於指定的用戶
                if (setting.UserId != userId)
                {
                    return NotFound($"Setting with ID {id} not found for user ID {userId}");
                }

                return Ok(setting);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Setting not found with ID: {Id} for user ID: {UserId}", id, userId);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving setting with ID: {Id} for user ID: {UserId}", id, userId);
                return StatusCode(500, "An error occurred while retrieving the user setting");
            }
        }

        [HttpGet("key/{key}")]
        public async Task<ActionResult<UserSettingResponseDto>> GetUserSettingByKey(int userId, string key)
        {
            try
            {
                // TODO: 驗證當前用戶只能查看自己的設定，或者是管理員

                var setting = await _userSettingService.GetUserSettingByKeyAsync(userId, key);
                return Ok(setting);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid key provided: {Key} for user ID: {UserId}", key, userId);
                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Setting not found with key: {Key} for user ID: {UserId}", key, userId);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving setting with key: {Key} for user ID: {UserId}", key, userId);
                return StatusCode(500, "An error occurred while retrieving the user setting");
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserSettingResponseDto>> CreateUserSetting(int userId, [FromBody] UserSettingCreateDto createSettingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 驗證當前用戶只能為自己創建設定，或者是管理員
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");
                
                if (currentUserId != userId && !isAdmin)
                {
                    return Forbid("You can only create settings for your own account");
                }

                var createdSetting = await _userSettingService.CreateUserSettingAsync(userId, createSettingDto);
                
                return CreatedAtAction(
                    nameof(GetUserSettingById),
                    new { userId = userId, id = createdSetting.Id },
                    createdSetting
                );
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null setting data provided for user ID: {UserId}", userId);
                return BadRequest("Setting data cannot be null");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid setting data provided for user ID: {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning(ex, "Attempted to create duplicate setting for user ID: {UserId}", userId);
                return Conflict(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for setting creation with ID: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while creating setting for user ID: {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the user setting");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserSettingResponseDto>> UpdateUserSetting(int userId, int id, [FromBody] UserSettingUpdateDto updateSettingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 先獲取現有設定以驗證它是否屬於指定的用戶
                var existingSetting = await _userSettingService.GetUserSettingByIdAsync(id);
                if (existingSetting.UserId != userId)
                {
                    return NotFound($"Setting with ID {id} not found for user ID {userId}");
                }

                // 驗證當前用戶只能更新自己的設定，或者是管理員
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");
                
                if (currentUserId != userId && !isAdmin)
                {
                    return Forbid("You can only update your own settings");
                }

                var updatedSetting = await _userSettingService.UpdateUserSettingAsync(id, updateSettingDto);
                return Ok(updatedSetting);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null setting data provided for update for user ID: {UserId}", userId);
                return BadRequest("Setting data cannot be null");
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Setting not found for update with ID: {Id} for user ID: {UserId}", id, userId);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while updating setting with ID: {Id} for user ID: {UserId}", id, userId);
                return StatusCode(500, "An error occurred while updating the user setting");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserSetting(int userId, int id)
        {
            try
            {
                // 先獲取現有設定以驗證它是否屬於指定的用戶
                var existingSetting = await _userSettingService.GetUserSettingByIdAsync(id);
                if (existingSetting.UserId != userId)
                {
                    return NotFound($"Setting with ID {id} not found for user ID {userId}");
                }

                // 驗證當前用戶只能刪除自己的設定，或者是管理員
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");
                
                if (currentUserId != userId && !isAdmin)
                {
                    return Forbid("You can only delete your own settings");
                }

                await _userSettingService.DeleteUserSettingAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Setting not found for deletion with ID: {Id} for user ID: {UserId}", id, userId);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting setting with ID: {Id} for user ID: {UserId}", id, userId);
                return StatusCode(500, "An error occurred while deleting the user setting");
            }
        }
    }
}