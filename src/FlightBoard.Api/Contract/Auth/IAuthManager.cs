using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.Contract.Auth;

/// <summary>
/// Public contract for Authentication Manager - Use case orchestration interface
/// Following iDesign Method: Only public manager contracts are in Contract namespace
/// </summary>
public interface IAuthManager
{
    // Authentication operations
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(int userId);

    // User profile operations
    Task<UserDto?> GetUserProfileAsync(int userId);
    Task<UserDto?> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

    // Validation operations
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
}
