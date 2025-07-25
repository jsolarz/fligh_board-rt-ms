using FlightBoard.Api.Models;
using UserModel = FlightBoard.Api.Models.User;

namespace FlightBoard.Api.DataAccess.User;

/// <summary>
/// User data access contract
/// Following iDesign Method: Data access layer interface
/// </summary>
public interface IUserDataAccess
{
    // Basic CRUD operations
    Task<UserModel?> GetByIdAsync(int id);
    Task<UserModel?> GetByUsernameAsync(string username);
    Task<UserModel?> GetByEmailAsync(string email);
    Task<List<UserModel>> GetAllAsync();
    Task<UserModel> CreateAsync(UserModel user);
    Task<UserModel> UpdateAsync(UserModel user);
    Task<bool> DeleteAsync(int id);

    // Authentication specific methods
    Task<bool> ExistsAsync(string username, string email);
    Task<bool> UpdateLastLoginAsync(int userId, DateTime loginTime);
    Task<bool> UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiryTime);
    Task<UserModel?> GetByRefreshTokenAsync(string refreshToken);

    // User management
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<List<UserModel>> GetActiveUsersAsync();
    Task<bool> ActivateUserAsync(int userId);
    Task<bool> DeactivateUserAsync(int userId);
}
