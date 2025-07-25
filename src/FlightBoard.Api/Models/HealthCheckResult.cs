namespace FlightBoard.Api.Models;

/// <summary>
/// Health check result model for API health endpoints
/// </summary>
public class HealthCheckResult
{
    public string Status { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public bool Connected { get; set; }
    public string? Provider { get; set; }
    public int FlightCount { get; set; }
    public int UserCount { get; set; }
    public string? Error { get; set; }
}
