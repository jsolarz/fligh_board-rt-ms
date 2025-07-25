using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FlightBoard.Api.Models;
using FlightBoard.Api.iFX.Contract;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// JWT service implementation for token generation and validation
/// Following iDesign Method: Infrastructure framework service implementation
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();

        var jwtSecret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    }

    /// <summary>
    /// Generates a JWT access token for the specified user
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("full_name", user.FullName),
            new("is_active", user.IsActive.ToString()),
            new("jti", Guid.NewGuid().ToString()) // JWT ID for token uniqueness
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Validates a JWT token and returns the principal
    /// </summary>
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        try
        {
            var tokenValidationParameters = GetTokenValidationParameters();

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            // Ensure the token is a JWT token
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates a JWT token without checking expiry (used for refresh token validation)
    /// </summary>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenValidationParameters = GetTokenValidationParameters();
            tokenValidationParameters.ValidateLifetime = false; // Don't validate expiry

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            // Ensure the token is a JWT token
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the user ID from a JWT token
    /// </summary>
    public int? GetUserIdFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets token validation parameters for JWT validation
    /// </summary>
    private TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero // No clock skew tolerance
        };
    }

    /// <summary>
    /// Gets access token expiry time in minutes from configuration
    /// </summary>
    private int GetAccessTokenExpiryMinutes()
    {
        return _configuration.GetValue<int?>("Jwt:AccessTokenExpiryMinutes") ?? 15; // Default 15 minutes
    }
}
