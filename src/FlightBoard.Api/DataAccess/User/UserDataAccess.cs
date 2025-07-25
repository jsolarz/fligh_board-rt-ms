using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Models;
using FlightBoard.Api.Data;
using UserModel = FlightBoard.Api.Models.User;

namespace FlightBoard.Api.DataAccess.User;

/// <summary>
/// User data access implementation using Entity Framework
/// Following iDesign Method: Data access layer implementation
/// </summary>
public class UserDataAccess : IUserDataAccess
{
    private readonly FlightDbContext _context;

    public UserDataAccess(FlightDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    public async Task<UserModel?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Gets a user by username
    /// </summary>
    public async Task<UserModel?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>
    /// Gets a user by email
    /// </summary>
    public async Task<UserModel?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    public async Task<List<UserModel>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public async Task<UserModel> CreateAsync(UserModel user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    public async Task<UserModel> UpdateAsync(UserModel user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Deletes a user (soft delete via IsDeleted flag)
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;

        user.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a username or email already exists
    /// </summary>
    public async Task<bool> ExistsAsync(string username, string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() ||
                          u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Updates the last login time for a user
    /// </summary>
    public async Task<bool> UpdateLastLoginAsync(int userId, DateTime loginTime)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;

        user.LastLoginAt = loginTime;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Updates the refresh token for a user
    /// </summary>
    public async Task<bool> UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiryTime)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiryTime;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets a user by refresh token
    /// </summary>
    public async Task<UserModel?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                    u.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    /// <summary>
    /// Checks if a username is available
    /// </summary>
    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return !await _context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>
    /// Checks if an email is available
    /// </summary>
    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Gets all active users
    /// </summary>
    public async Task<List<UserModel>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    /// <summary>
    /// Activates a user
    /// </summary>
    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deactivates a user
    /// </summary>
    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
