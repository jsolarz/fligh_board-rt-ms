using System.Security.Cryptography;
using System.Text;
using FlightBoard.Api.iFX.Contract;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Password hashing service implementation using PBKDF2
/// Following iDesign Method: Infrastructure framework service implementation
/// </summary>
public class PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 32; // 256 bits
    private const int KeySize = 64; // 512 bits
    private const int Iterations = 50000; // OWASP recommended minimum
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    /// <summary>
    /// Hashes a password using PBKDF2 with SHA512
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate a random salt
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        // Hash the password
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            KeySize);

        // Combine salt and hash for storage
        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // Decode the hash
            var hashBytes = Convert.FromBase64String(hash);

            // Ensure the hash is the correct length
            if (hashBytes.Length != SaltSize + KeySize)
                return false;

            // Extract the salt
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract the stored hash
            var storedHash = new byte[KeySize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, KeySize);

            // Hash the provided password with the same salt
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                Algorithm,
                KeySize);

            // Compare the hashes using a timing-safe comparison
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
}
