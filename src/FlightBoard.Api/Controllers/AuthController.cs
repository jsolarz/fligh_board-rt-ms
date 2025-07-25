using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FlightBoard.Api.Contract.Auth;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// Authentication controller for user management and JWT token operations
/// Following iDesign Method: Controllers call public manager contracts
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthManager authManager, ILogger<AuthController> logger)
    {
        _authManager = authManager;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - authenticate user and return JWT tokens
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authManager.LoginAsync(loginDto);

            if (result == null)
            {
                _logger.LogWarning("Login attempt failed for username: {Username}", loginDto.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for username: {Username}", loginDto.Username);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Registration endpoint - create new user account
    /// </summary>
    /// <param name="registerDto">Registration details</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var result = await _authManager.RegisterAsync(registerDto);

            if (result == null)
            {
                _logger.LogWarning("Registration failed for username: {Username}", registerDto.Username);
                return BadRequest(new { message = "Registration failed. Username or email may already be taken." });
            }

            _logger.LogInformation("User {Username} registered successfully", registerDto.Username);
            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", registerDto.Username);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Token refresh endpoint - get new access token using refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token details</param>
    /// <returns>New authentication response with fresh tokens</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var result = await _authManager.RefreshTokenAsync(refreshTokenDto);

            if (result == null)
            {
                _logger.LogWarning("Token refresh failed - invalid or expired tokens");
                return Unauthorized(new { message = "Invalid or expired tokens" });
            }

            _logger.LogInformation("Token refreshed successfully for user {UserId}", result.User.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logout endpoint - invalidate refresh token
    /// </summary>
    /// <returns>Success confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _authManager.LogoutAsync(userId.Value);

            if (!success)
            {
                return BadRequest(new { message = "Logout failed" });
            }

            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _authManager.GetUserProfileAsync(userId.Value);

            if (user == null)
            {
                return NotFound(new { message = "User profile not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    /// <param name="updateDto">Updated profile information</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _authManager.UpdateUserProfileAsync(userId.Value, updateDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to update profile" });
            }

            _logger.LogInformation("Profile updated successfully for user {UserId}", userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="changePasswordDto">Password change details</param>
    /// <returns>Success confirmation</returns>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _authManager.ChangePasswordAsync(userId.Value, changePasswordDto);

            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Current password may be incorrect." });
            }

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    /// <summary>
    /// Check if username is available
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>Availability status</returns>
    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<bool>> CheckUsernameAvailability(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                return BadRequest(new { message = "Username must be at least 3 characters long" });
            }

            var isAvailable = await _authManager.IsUsernameAvailableAsync(username);
            return Ok(new { available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability");
            return StatusCode(500, new { message = "An error occurred while checking username" });
        }
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>Availability status</returns>
    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmailAvailability([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            var isAvailable = await _authManager.IsEmailAvailableAsync(email);
            return Ok(new { available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability");
            return StatusCode(500, new { message = "An error occurred while checking email" });
        }
    }

    /// <summary>
    /// Gets the current user's ID from JWT token
    /// </summary>
    /// <returns>User ID or null if not found</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
