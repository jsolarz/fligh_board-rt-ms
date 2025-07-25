namespace FlightBoard.Api.iFX.Contract;

/// <summary>
/// Password hashing service contract
/// Following iDesign Method: Infrastructure framework contracts
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// Hashes a password using a secure algorithm
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hash);
}
