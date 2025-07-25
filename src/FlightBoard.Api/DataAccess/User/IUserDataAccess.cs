namespace FlightBoard.Api.DataAccess.User;

/// <summary>
/// User data access contract
/// Following iDesign Method: Data access layer interface
/// </summary>
public interface IUserDataAccess
{
    // Basic CRUD operations
    Task<FlightBoard.Api.Core.Entities.User?> GetByIdAsync(int id);
    Task<FlightBoard.Api.Core.Entities.User?> GetByUsernameAsync(string username);
    Task<FlightBoard.Api.Core.Entities.User?> GetByEmailAsync(string email);
    Task<List<FlightBoard.Api.Core.Entities.User>> GetAllAsync();
    Task<FlightBoard.Api.Core.Entities.User> CreateAsync(FlightBoard.Api.Core.Entities.User user);
    Task<FlightBoard.Api.Core.Entities.User> UpdateAsync(FlightBoard.Api.Core.Entities.User user);
    Task<bool> DeleteAsync(int id);

    // Authentication specific methods
    Task<bool> ExistsAsync(string username, string email);
    Task<bool> UpdateLastLoginAsync(int userId, DateTime loginTime);
    Task<bool> UpdateRefreshTokenAsync(int userId, string? refreshToken, DateTime? expiryTime);
    Task<FlightBoard.Api.Core.Entities.User?> GetByRefreshTokenAsync(string refreshToken);

    // User management
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<List<FlightBoard.Api.Core.Entities.User>> GetActiveUsersAsync();
    Task<bool> ActivateUserAsync(int userId);
    Task<bool> DeactivateUserAsync(int userId);
}
