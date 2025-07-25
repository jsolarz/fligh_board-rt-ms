using System.ComponentModel.DataAnnotations;

namespace FlightBoard.Api.DTOs;

/// <summary>
/// Login request DTO with validation
/// </summary>
public record LoginDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters", MinimumLength = 3)]
    public required string Username { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters", MinimumLength = 6)]
    public required string Password { get; init; }
}

/// <summary>
/// Registration request DTO with comprehensive validation
/// </summary>
public record RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters", MinimumLength = 3)]
    public required string Username { get; init; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters", MinimumLength = 6)]
    public required string Password { get; init; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    public required string FirstName { get; init; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    public required string LastName { get; init; }
}

/// <summary>
/// Authentication response DTO containing JWT tokens and user information
/// </summary>
public record AuthResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required UserDto User { get; init; }
}

/// <summary>
/// User information DTO for client applications
/// </summary>
public record UserDto
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Role { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsAdmin => Role == "Admin";
    public bool IsOperator => Role == "Operator";
}

/// <summary>
/// Token refresh request DTO
/// </summary>
public record RefreshTokenDto
{
    [Required(ErrorMessage = "Access token is required")]
    public required string AccessToken { get; init; }

    [Required(ErrorMessage = "Refresh token is required")]
    public required string RefreshToken { get; init; }
}

/// <summary>
/// Change password request DTO
/// </summary>
public record ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required")]
    public required string CurrentPassword { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters", MinimumLength = 6)]
    public required string NewPassword { get; init; }
}

/// <summary>
/// Update user profile DTO
/// </summary>
public record UpdateUserProfileDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    public required string FirstName { get; init; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    public required string LastName { get; init; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public required string Email { get; init; }
}
