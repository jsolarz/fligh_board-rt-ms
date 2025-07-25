using System.ComponentModel.DataAnnotations;

namespace FlightBoard.Api.Core.Entities;

/// <summary>
/// User entity for authentication and role-based access control
/// Following IDesign Method: Domain models with proper validation and audit
/// </summary>
public class User : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    [StringLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsOperator => Role == UserRole.Operator;
}

/// <summary>
/// User roles for role-based access control
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard user - read-only access to flight information
    /// </summary>
    User = 0,

    /// <summary>
    /// Flight operator - can update flight status and basic information
    /// </summary>
    Operator = 1,

    /// <summary>
    /// System administrator - full CRUD access to flights and user management
    /// </summary>
    Admin = 2
}
