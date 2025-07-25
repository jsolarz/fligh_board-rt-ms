using System.ComponentModel.DataAnnotations;

namespace FlightBoard.Api.Core.Entities;

/// <summary>
/// Base entity class with common audit fields for all entities
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Creation timestamp with UTC timezone
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp with UTC timezone
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag - true indicates the record is deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Optional user identifier who created the record
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Optional user identifier who last updated the record
    /// </summary>
    public string? UpdatedBy { get; set; }
}
