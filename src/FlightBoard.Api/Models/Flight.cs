using System.ComponentModel.DataAnnotations;

namespace FlightBoard.Api.Models;

/// <summary>
/// Flight entity representing flight information with real-time status updates
/// </summary>
public class Flight : BaseEntity
{
    /// <summary>
    /// Flight number (e.g., "AA123", "DL456")
    /// </summary>
    [Required]
    [StringLength(10)]
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Airline code (e.g., "AA" for American Airlines)
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Airline { get; set; } = string.Empty;

    /// <summary>
    /// Origin airport code (e.g., "JFK", "LAX")
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Destination airport code (e.g., "JFK", "LAX")
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled departure time in UTC
    /// </summary>
    public DateTime ScheduledDeparture { get; set; }

    /// <summary>
    /// Actual departure time in UTC (null if not departed)
    /// </summary>
    public DateTime? ActualDeparture { get; set; }

    /// <summary>
    /// Scheduled arrival time in UTC
    /// </summary>
    public DateTime ScheduledArrival { get; set; }

    /// <summary>
    /// Actual arrival time in UTC (null if not arrived)
    /// </summary>
    public DateTime? ActualArrival { get; set; }

    /// <summary>
    /// Current flight status
    /// </summary>
    [Required]
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

    /// <summary>
    /// Gate number for departure/arrival (e.g., "A12", "B5")
    /// </summary>
    [StringLength(10)]
    public string? Gate { get; set; }

    /// <summary>
    /// Terminal information (e.g., "Terminal 1", "T2")
    /// </summary>
    [StringLength(20)]
    public string? Terminal { get; set; }

    /// <summary>
    /// Aircraft type (e.g., "Boeing 737", "Airbus A320")
    /// </summary>
    [StringLength(50)]
    public string? AircraftType { get; set; }

    /// <summary>
    /// Additional remarks or announcements
    /// </summary>
    [StringLength(500)]
    public string? Remarks { get; set; }

    /// <summary>
    /// Delay duration in minutes (positive for delays, negative for early)
    /// </summary>
    public int DelayMinutes { get; set; } = 0;

    /// <summary>
    /// Flight type - Arrival or Departure
    /// </summary>
    [Required]
    public FlightType Type { get; set; }

    /// <summary>
    /// Calculate if flight is delayed (departure delay > 15 minutes)
    /// </summary>
    public bool IsDelayed => DelayMinutes > 15;

    /// <summary>
    /// Get estimated departure time (scheduled + delay)
    /// </summary>
    public DateTime EstimatedDeparture => ScheduledDeparture.AddMinutes(DelayMinutes);

    /// <summary>
    /// Get estimated arrival time (scheduled + delay)
    /// </summary>
    public DateTime EstimatedArrival => ScheduledArrival.AddMinutes(DelayMinutes);
}

/// <summary>
/// Enumeration for flight status values
/// </summary>
public enum FlightStatus
{
    Scheduled,      // Flight is scheduled but not yet departed
    Boarding,       // Passengers are boarding
    Departed,       // Flight has departed
    InFlight,       // Flight is currently in the air
    Landed,         // Flight has landed
    Arrived,        // Flight has arrived at gate
    Delayed,        // Flight is delayed
    Cancelled,      // Flight is cancelled
    Diverted        // Flight has been diverted to another airport
}

/// <summary>
/// Enumeration for flight type (arrival or departure)
/// </summary>
public enum FlightType
{
    Departure,      // Outbound flight
    Arrival         // Inbound flight
}
