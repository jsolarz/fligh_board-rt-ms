using FlightBoard.Api.Contract.Auth;
using FlightBoard.Api.DTOs;
using FlightBoard.Api.Engines;
using FlightBoard.Api.DataAccess.User;
using FlightBoard.Api.iFX.Contract;
using FlightBoard.Api.Models;

namespace FlightBoard.Api.Managers;

/// <summary>
/// Authentication manager implementation - Use case orchestration
/// Following iDesign Method: Manager orchestrates Engine + DataAccess + CrossCutting
/// </summary>
public class AuthManager : IAuthManager
{
    private readonly IAuthEngine _authEngine;
    private readonly IUserDataAccess _userDataAccess;
    private readonly IJwtService _jwtService;
    private readonly IUserMappingUtility _userMappingUtility;
    private readonly IConfiguration _configuration;

    public AuthManager(
        IAuthEngine authEngine,
        IUserDataAccess userDataAccess,
        IJwtService jwtService,
        IUserMappingUtility userMappingUtility,
        IConfiguration configuration)
    {
        _authEngine = authEngine;
        _userDataAccess = userDataAccess;
        _jwtService = jwtService;
        _userMappingUtility = userMappingUtility;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        // Validate credentials using engine
        var (success, user, errorMessage) = await _authEngine.ValidateUserCredentialsAsync(loginDto.Username, loginDto.Password);

        if (!success || user == null)
            return null;

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

        // Store refresh token
        await _userDataAccess.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        // Update last login time
        await _userDataAccess.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

        // Return authentication response
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            User = _userMappingUtility.ToUserDto(user)
        };
    }

    /// <summary>
    /// Registers a new user and returns JWT tokens
    /// </summary>
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Register user using engine
        var (success, user, errorMessage) = await _authEngine.RegisterUserAsync(registerDto);

        if (!success || user == null)
            return null;

        // Generate tokens for immediate login
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

        // Store refresh token
        await _userDataAccess.UpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        // Return authentication response
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            User = _userMappingUtility.ToUserDto(user)
        };
    }

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        // Validate the expired access token
        var principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        if (principal == null)
            return null;

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return null;

        // Validate refresh token
        if (!await _authEngine.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken))
            return null;

        // Get user
        var user = await _userDataAccess.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
            return null;

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

        // Update refresh token
        await _userDataAccess.UpdateRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            User = _userMappingUtility.ToUserDto(user)
        };
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    public async Task<bool> LogoutAsync(int userId)
    {
        return await _authEngine.InvalidateRefreshTokenAsync(userId);
    }

    /// <summary>
    /// Gets user profile information
    /// </summary>
    public async Task<UserDto?> GetUserProfileAsync(int userId)
    {
        var user = await _userDataAccess.GetByIdAsync(userId);
        return user is not null ? _userMappingUtility.ToUserDto(user) : null;
    }

    /// <summary>
    /// Updates user profile information
    /// </summary>
    public async Task<UserDto?> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
    {
        var (success, user, errorMessage) = await _authEngine.UpdateUserProfileAsync(userId, updateDto);
        return success && user != null ? _userMappingUtility.ToUserDto(user) : null;
    }

    /// <summary>
    /// Changes a user's password
    /// </summary>
    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        return await _authEngine.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
    }

    /// <summary>
    /// Checks if a username is available
    /// </summary>
    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return await _authEngine.IsUsernameAvailableAsync(username);
    }

    /// <summary>
    /// Checks if an email is available
    /// </summary>
    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return await _authEngine.IsEmailAvailableAsync(email);
    }

    /// <summary>
    /// Gets access token expiry time in minutes from configuration
    /// </summary>
    private int GetAccessTokenExpiryMinutes()
    {
        return _configuration.GetValue<int?>("Jwt:AccessTokenExpiryMinutes") ?? 15;
    }

    /// <summary>
    /// Gets refresh token expiry time in days from configuration
    /// </summary>
    private int GetRefreshTokenExpiryDays()
    {
        return _configuration.GetValue<int?>("Jwt:RefreshTokenExpiryDays") ?? 7;
    }
}
