using FlightBoard.Api.Models;

namespace FlightBoard.Api.iFX.Utilities.Extensions;

/// <summary>
/// Extension methods for Flight entity operations
/// </summary>
public static class FlightExtensions
{
    /// <summary>
    /// Check if flight is currently active (boarding or recently departed)
    /// </summary>
    public static bool IsActive(this Flight flight)
    {
        return flight.Status == FlightStatus.Boarding ||
               flight.Status == FlightStatus.Departed ||
               flight.ScheduledDeparture.IsDepartingSoon() ||
               flight.ActualDeparture.HasRecentlyDeparted();
    }

    /// <summary>
    /// Check if flight is delayed
    /// </summary>
    public static bool IsDelayed(this Flight flight)
    {
        return flight.DelayMinutes > 0 || flight.Status == FlightStatus.Delayed;
    }

    /// <summary>
    /// Get effective departure time (actual if available, otherwise scheduled + delay)
    /// </summary>
    public static DateTime GetEffectiveDepartureTime(this Flight flight)
    {
        return flight.ActualDeparture ?? flight.ScheduledDeparture.AddMinutes(flight.DelayMinutes);
    }

    /// <summary>
    /// Get effective arrival time (actual if available, otherwise scheduled + delay)
    /// </summary>
    public static DateTime GetEffectiveArrivalTime(this Flight flight)
    {
        return flight.ActualArrival ?? flight.ScheduledArrival.AddMinutes(flight.DelayMinutes);
    }

    /// <summary>
    /// Check if flight matches search criteria
    /// </summary>
    public static bool MatchesSearchCriteria(this Flight flight, string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return true;

        var term = searchTerm.ToLowerInvariant();
        return flight.FlightNumber.ToLowerInvariant().Contains(term) ||
               flight.Airline.ToLowerInvariant().Contains(term) ||
               flight.Origin.ToLowerInvariant().Contains(term) ||
               flight.Destination.ToLowerInvariant().Contains(term) ||
               (flight.Gate?.ToLowerInvariant().Contains(term) ?? false);
    }
}
