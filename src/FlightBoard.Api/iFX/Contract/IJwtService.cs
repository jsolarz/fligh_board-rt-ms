using FlightBoard.Api.Models;

namespace FlightBoard.Api.iFX.Contract;

/// <summary>
/// JWT service contract for token generation and validation
/// Following iDesign Method: Infrastructure framework contracts
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT access token for the specified user
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token for token renewal
    /// </summary>
    /// <returns>Random refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns the principal
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Claims principal if valid, null if invalid</returns>
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromToken(string token);

    /// <summary>
    /// Validates a JWT token without checking expiry (used for refresh token validation)
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Claims principal if valid, null if invalid</returns>
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Gets the user ID from a JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID if found, null otherwise</returns>
    int? GetUserIdFromToken(string token);
}
