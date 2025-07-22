using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;

namespace FlightBoard.Api.Engines;

/// <summary>
/// Authentication engine contract for business logic
/// Following iDesign Method: Business logic layer interface
/// </summary>
public interface IAuthEngine
{
    // Core authentication operations
    Task<(bool Success, Models.User? User, string? ErrorMessage)> ValidateUserCredentialsAsync(string username, string password);
    Task<(bool Success, Models.User? User, string? ErrorMessage)> RegisterUserAsync(RegisterDto registerDto);

    // Token validation
    Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    Task<bool> InvalidateRefreshTokenAsync(int userId);

    // User management
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<(bool Success, Models.User? User, string? ErrorMessage)> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);

    // User validation
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);

    // Security operations
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
