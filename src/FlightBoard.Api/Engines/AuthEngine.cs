using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;
using FlightBoard.Api.DataAccess.User;
using FlightBoard.Api.iFX.Contract;

namespace FlightBoard.Api.Engines;

/// <summary>
/// Authentication engine implementation containing pure business logic
/// Following iDesign Method: Business logic layer implementation
/// </summary>
public class AuthEngine : IAuthEngine
{
    private readonly IUserDataAccess _userDataAccess;
    private readonly IPasswordHashService _passwordHashService;

    public AuthEngine(IUserDataAccess userDataAccess, IPasswordHashService passwordHashService)
    {
        _userDataAccess = userDataAccess;
        _passwordHashService = passwordHashService;
    }

    /// <summary>
    /// Validates user credentials for login
    /// </summary>
    public async Task<(bool Success, Models.User? User, string? ErrorMessage)> ValidateUserCredentialsAsync(string username, string password)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, null, "Username and password are required");
        }

        // Get user by username
        var user = await _userDataAccess.GetByUsernameAsync(username);
        if (user == null)
        {
            return (false, null, "Invalid username or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return (false, null, "User account is deactivated");
        }

        // Verify password
        if (!_passwordHashService.VerifyPassword(password, user.PasswordHash))
        {
            return (false, null, "Invalid username or password");
        }

        return (true, user, null);
    }

    /// <summary>
    /// Registers a new user with business rule validation
    /// </summary>
    public async Task<(bool Success, Models.User? User, string? ErrorMessage)> RegisterUserAsync(RegisterDto registerDto)
    {
        // Check if username is available
        if (!await _userDataAccess.IsUsernameAvailableAsync(registerDto.Username))
        {
            return (false, null, "Username is already taken");
        }

        // Check if email is available
        if (!await _userDataAccess.IsEmailAvailableAsync(registerDto.Email))
        {
            return (false, null, "Email is already registered");
        }

        // Business rule: Password strength validation (additional to data annotations)
        if (!IsPasswordStrong(registerDto.Password))
        {
            return (false, null, "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
        }

        // Create new user
        var user = new Models.User
        {
            Username = registerDto.Username.Trim(),
            Email = registerDto.Email.Trim().ToLowerInvariant(),
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim(),
            PasswordHash = _passwordHashService.HashPassword(registerDto.Password),
            Role = UserRole.User, // Default role
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var createdUser = await _userDataAccess.CreateAsync(user);
            return (true, createdUser, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Failed to create user: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a refresh token for a user
    /// </summary>
    public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var user = await _userDataAccess.GetByRefreshTokenAsync(refreshToken);
        return user != null && user.Id == userId && user.IsActive;
    }

    /// <summary>
    /// Invalidates a user's refresh token
    /// </summary>
    public async Task<bool> InvalidateRefreshTokenAsync(int userId)
    {
        return await _userDataAccess.UpdateRefreshTokenAsync(userId, null, null);
    }

    /// <summary>
    /// Changes a user's password with current password verification
    /// </summary>
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userDataAccess.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
            return false;

        // Verify current password
        if (!_passwordHashService.VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        // Business rule: Password strength validation
        if (!IsPasswordStrong(newPassword))
            return false;

        // Business rule: New password must be different from current
        if (_passwordHashService.VerifyPassword(newPassword, user.PasswordHash))
            return false;

        // Update password
        user.PasswordHash = _passwordHashService.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _userDataAccess.UpdateAsync(user);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Updates a user's profile information
    /// </summary>
    public async Task<(bool Success, Models.User? User, string? ErrorMessage)> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
    {
        var user = await _userDataAccess.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, null, "User not found");
        }

        if (!user.IsActive)
        {
            return (false, null, "User account is deactivated");
        }

        // Check if email is available (excluding current user)
        var existingEmailUser = await _userDataAccess.GetByEmailAsync(updateDto.Email);
        if (existingEmailUser != null && existingEmailUser.Id != userId)
        {
            return (false, null, "Email is already registered to another user");
        }

        // Update user information
        user.FirstName = updateDto.FirstName.Trim();
        user.LastName = updateDto.LastName.Trim();
        user.Email = updateDto.Email.Trim().ToLowerInvariant();
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            var updatedUser = await _userDataAccess.UpdateAsync(user);
            return (true, updatedUser, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Failed to update profile: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a username is available
    /// </summary>
    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return await _userDataAccess.IsUsernameAvailableAsync(username);
    }

    /// <summary>
    /// Checks if an email is available
    /// </summary>
    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return await _userDataAccess.IsEmailAvailableAsync(email);
    }

    /// <summary>
    /// Hashes a password using the configured service
    /// </summary>
    public string HashPassword(string password)
    {
        return _passwordHashService.HashPassword(password);
    }

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        return _passwordHashService.VerifyPassword(password, hash);
    }

    /// <summary>
    /// Business rule: Validates password strength
    /// Must contain at least one uppercase, lowercase, digit, and special character
    /// </summary>
    private static bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return false;

        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (!char.IsWhiteSpace(c)) hasSpecial = true;
        }

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
