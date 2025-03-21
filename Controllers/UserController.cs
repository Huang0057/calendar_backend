using Microsoft.AspNetCore.Mvc;
using Calendar.API.DTOs.UserDtos;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace Calendar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] // 只有管理員可以查看所有用戶
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        [HttpGet("username/{username}")]
        //[Authorize]
        public async Task<ActionResult<UserResponseDto>> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid username provided: {Username}", username);
                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found with username: {Username}", username);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with username: {Username}", username);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserCreateDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdUser = await _userService.CreateUserAsync(createUserDto);
                
                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = createdUser.Id },
                    createdUser
                );
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null user data provided");
                return BadRequest("User data cannot be null");
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning(ex, "Attempted to create user with duplicate username or email");
                return Conflict(ex.Message);
            }
            catch (BusinessValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid user data provided");
                return BadRequest(ex.Message);
            }

            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        [HttpPut("{id}")]
        //[Authorize]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, [FromBody] UserUpdateDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 驗證當前用戶只能更新自己的資料，或者是管理員
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid("You can only update your own profile");
                }

                var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
                return Ok(updatedUser);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null user data provided for update");
                return BadRequest("User data cannot be null");
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for update with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning(ex, "Attempted to update to duplicate username or email");
                return Conflict(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        [HttpPut("{id}/change-password")]
        //[Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] UserChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 驗證當前用戶只能更新自己的密碼，或者是管理員
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var isAdmin = User.IsInRole("Admin");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid("You can only change your own password");
                }

                await _userService.ChangePasswordAsync(id, changePasswordDto);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null password data provided");
                return BadRequest("Password data cannot be null");
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for password change with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (BusinessValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid password data provided");
                return BadRequest(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user with ID: {Id}", id);
                return StatusCode(500, "An error occurred while changing the password");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // 只有管理員可以刪除用戶
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for deletion with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tokenResponse = await _userService.LoginAsync(loginDto);
                return Ok(tokenResponse);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null login data provided");
                return BadRequest("Login data cannot be null");
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Authentication failed");
                return Unauthorized(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                return StatusCode(500, "An error occurred during login");
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tokenResponse = await _userService.RefreshTokenAsync(refreshTokenDto);
                return Ok(tokenResponse);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null refresh token data provided");
                return BadRequest("Refresh token data cannot be null");
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Token refresh failed");
                return Unauthorized(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return StatusCode(500, "An error occurred during token refresh");
            }
        }

        [HttpPost("logout")]
        //[Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // 從 JWT token 中獲取用戶 ID
                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                
                if (userId == 0)
                {
                    return BadRequest("User ID not found in token");
                }

                await _userService.LogoutAsync(userId);
                return NoContent();
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return StatusCode(500, "An error occurred during logout");
            }
        }
    }
}