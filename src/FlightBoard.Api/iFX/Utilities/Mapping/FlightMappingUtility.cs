using FlightBoard.Api.Core.Entities;
using FlightBoard.Api.Core.DTOs;

namespace FlightBoard.Api.iFX.Utilities.Mapping;

/// <summary>
/// Flight mapping utility following iDesign Method principles
/// Handles mapping between entities and DTOs
/// </summary>
public class FlightMappingUtility : IFlightMappingUtility
{
    /// <summary>
    /// Map Flight entity to DTO with calculated status
    /// </summary>
    public FlightDto MapToDto(Flight flight, FlightStatus calculatedStatus)
    {
        var scheduledDeparture = flight.ScheduledDeparture;
        var actualDeparture = flight.ActualDeparture ?? scheduledDeparture.AddMinutes(flight.DelayMinutes);
        var scheduledArrival = flight.ScheduledArrival;
        var actualArrival = flight.ActualArrival ?? scheduledArrival.AddMinutes(flight.DelayMinutes);

        return new FlightDto
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline,
            Origin = flight.Origin,
            Destination = flight.Destination,
            ScheduledDeparture = scheduledDeparture,
            ActualDeparture = flight.ActualDeparture,
            ScheduledArrival = scheduledArrival,
            ActualArrival = flight.ActualArrival,
            Status = calculatedStatus.ToString(),
            Gate = flight.Gate,
            Terminal = flight.Terminal,
            AircraftType = flight.AircraftType,
            Remarks = flight.Remarks,
            DelayMinutes = flight.DelayMinutes,
            Type = DetermineFlightType(scheduledDeparture),
            CreatedAt = flight.CreatedAt,
            UpdatedAt = flight.UpdatedAt,
            IsDelayed = flight.DelayMinutes > 0 || calculatedStatus == FlightStatus.Delayed,
            EstimatedDeparture = actualDeparture,
            EstimatedArrival = actualArrival
        };
    }

    /// <summary>
    /// Map Flight entity to DTO using the stored status
    /// </summary>
    public FlightDto MapToDto(Flight flight)
    {
        return MapToDto(flight, flight.Status);
    }

    /// <summary>
    /// Determine flight type based on departure time
    /// </summary>
    private static string DetermineFlightType(DateTime scheduledDeparture)
    {
        // Simple logic: flights within next 24 hours are departures, others are arrivals
        var now = DateTime.UtcNow;
        var hoursUntilDeparture = (scheduledDeparture - now).TotalHours;

        return hoursUntilDeparture <= 24 ? "Departure" : "Arrival";
    }
}
