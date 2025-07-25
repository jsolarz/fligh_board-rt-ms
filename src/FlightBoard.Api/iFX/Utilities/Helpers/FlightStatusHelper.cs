using FlightBoard.Api.Core.Entities;

namespace FlightBoard.Api.iFX.Utilities.Helpers;

/// <summary>
/// Helper class for flight status calculations and business rules
/// </summary>
public static class FlightStatusHelper
{
    /// <summary>
    /// Calculate flight status based on business rules
    /// </summary>
    public static FlightStatus CalculateStatus(DateTime scheduledDeparture, DateTime? actualDeparture = null)
    {
        var now = DateTime.UtcNow;

        // If flight has actually departed, determine if it's landed
        if (actualDeparture.HasValue)
        {
            var minutesSinceDeparture = (now - actualDeparture.Value).TotalMinutes;
            return minutesSinceDeparture switch
            {
                <= 60 => FlightStatus.Departed,    // Departed within last 60 minutes
                > 60 => FlightStatus.Landed,       // More than 60 minutes after departure
                _ => FlightStatus.Departed
            };
        }

        // Calculate based on scheduled departure time
        var minutesUntilDeparture = (scheduledDeparture - now).TotalMinutes;

        return minutesUntilDeparture switch
        {
            > 30 => FlightStatus.Scheduled,        // More than 30 minutes before departure
            <= 30 and > 0 => FlightStatus.Boarding, // 30 minutes before to departure time
            <= 0 and > -60 => FlightStatus.Departed, // 0 to 60 minutes after scheduled departure
            <= -60 => FlightStatus.Landed,         // More than 60 minutes after scheduled departure
            _ => FlightStatus.Scheduled             // Default fallback
        };
    }

    /// <summary>
    /// Get user-friendly status description
    /// </summary>
    public static string GetStatusDescription(FlightStatus status)
    {
        return status switch
        {
            FlightStatus.Scheduled => "On Time",
            FlightStatus.Delayed => "Delayed",
            FlightStatus.Boarding => "Boarding",
            FlightStatus.Departed => "Departed",
            FlightStatus.Landed => "Landed",
            FlightStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Check if status represents an active flight
    /// </summary>
    public static bool IsActiveStatus(FlightStatus status)
    {
        return status == FlightStatus.Boarding || status == FlightStatus.Departed;
    }

    /// <summary>
    /// Check if status represents a completed flight
    /// </summary>
    public static bool IsCompletedStatus(FlightStatus status)
    {
        return status == FlightStatus.Landed || status == FlightStatus.Cancelled;
    }
}
