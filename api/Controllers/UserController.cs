using api.Dtos;
using api.Dtos.Users;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public UserController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        private IActionResult HandleNullResult<T>(T result, string notFoundMessage)
        {
            return result != null
                ? ApiResponse.Success(result, notFoundMessage.Replace("not found", "retrieved successfully"))
                : ApiResponse.NotFound(notFoundMessage);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] QueryParameters queryParams)
        {
            var result = await _userService.GetAllUsersAsync(queryParams);
            return result.Items.Any()
                ? ApiResponse.Success(result, "Users retrieved successfully.")
                : ApiResponse.NotFound("No users found.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var userDto = await _userService.GetUserByIdAsync(userId);
            return HandleNullResult(userDto, "User not found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserData)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid user data");
            }

            var newUser = await _userService.AddUserAsync(newUserData);
            return ApiResponse.Created(newUser, "User created successfully");
        }

        [Authorize]
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid user data provided.");
            }

            var updatedUserDto = await _userService.UpdateUserAsync(userId, updateUserDto);

            if (updatedUserDto == null)
            {
                return ApiResponse.NotFound($"User with ID {userId} not found.");
            }

            return ApiResponse.Success(updatedUserDto, "User updated successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("ban-unban/{userId}")]
        public async Task<IActionResult> BanUnbanUser(Guid userId)
        {
            var result = await _userService.BanUnbanUserAsync(userId);

            if (result)
            {
                return ApiResponse.Success($"User with this id {userId} has been banned.");
            }
            else
            {
                return ApiResponse.NotFound($"User with this id {userId} not found.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("banned-users")]
        public async Task<IActionResult> GetBannedUsers()
        {
            var users = await _userService.GetBannedUsersAsync();
            if (users.Count == 0)
            {
                return ApiResponse.NotFound("No banned users found.");
            }
            return ApiResponse.Success(users, "Banned users retrieved successfully.");
        }

        [Authorize]
        [HttpPost("reset-password/{userId}")]
        public async Task<IActionResult> ResetPassword(Guid userId, [FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid request data");
            }

            var result = await _userService.ResetPasswordAsync(userId, resetPasswordDto.NewPassword);
            if (!result)
            {
                return ApiResponse.NotFound($"User with ID {userId} not found.");
            }

            return ApiResponse.Success("", "Password reset successfully.");

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse.BadRequest("Invalid User Data");
            }

            var user = await _userService.LoginUserAsync(loginDto);
            if (user == null)
            {
                return ApiResponse.Unauthorized("Invalid credentials");
            }
            if (user.IsBanned)
            {
                return ApiResponse.Unauthorized("You are banned. Please contact us via email");
            }

            var token = _authService.GenerateJwtToken(user);
            return ApiResponse.Success(new { token, user }, "User logged in successfully");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return ApiResponse.Unauthorized("User ID is missing from token.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return ApiResponse.BadRequest("Invalid user ID format in token.");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            return HandleNullResult(user, "User not found");
        }
    }
}
