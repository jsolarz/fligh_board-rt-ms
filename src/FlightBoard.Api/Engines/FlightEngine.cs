using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.Engines;

/// <summary>
/// Flight business logic engine following iDesign Method principles
/// Contains all flight-related business rules and calculations
/// </summary>
public class FlightEngine : IFlightEngine
{
    private readonly ILogger<FlightEngine> _logger;

    public FlightEngine(ILogger<FlightEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculate flight status based on scheduled departure time and current time
    /// Implements business rules required by objectives.md
    /// </summary>
    /// <param name="scheduledDeparture">Scheduled departure time in UTC</param>
    /// <param name="actualDeparture">Actual departure time in UTC (if available)</param>
    /// <returns>Calculated flight status</returns>
    public FlightStatus CalculateFlightStatus(DateTime scheduledDeparture, DateTime? actualDeparture = null)
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
    /// Apply search filters to flight query
    /// </summary>
    public IQueryable<Flight> ApplySearchFilters(IQueryable<Flight> query, FlightSearchDto searchDto)
    {
        // Apply filters
        if (!string.IsNullOrEmpty(searchDto.FlightNumber))
            query = query.Where(f => f.FlightNumber.Contains(searchDto.FlightNumber));

        if (!string.IsNullOrEmpty(searchDto.Airline))
            query = query.Where(f => f.Airline.Contains(searchDto.Airline));

        if (!string.IsNullOrEmpty(searchDto.Origin))
            query = query.Where(f => f.Origin == searchDto.Origin);

        if (!string.IsNullOrEmpty(searchDto.Destination))
            query = query.Where(f => f.Destination == searchDto.Destination);

        if (!string.IsNullOrEmpty(searchDto.Status))
        {
            if (Enum.TryParse<FlightStatus>(searchDto.Status, true, out var status))
                query = query.Where(f => f.Status == status);
        }

        return query;
    }

    /// <summary>
    /// Validate flight data for creation
    /// </summary>
    public void ValidateFlightForCreation(CreateFlightDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.FlightNumber))
            throw new ArgumentException("Flight number is required", nameof(createDto.FlightNumber));

        if (string.IsNullOrWhiteSpace(createDto.Airline))
            throw new ArgumentException("Airline is required", nameof(createDto.Airline));

        if (createDto.ScheduledDeparture <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled departure must be in the future", nameof(createDto.ScheduledDeparture));

        _logger.LogDebug("Flight validation passed for creation: {FlightNumber}", createDto.FlightNumber);
    }

    /// <summary>
    /// Validate flight data for update
    /// </summary>
    public void ValidateFlightForUpdate(UpdateFlightDto updateDto, Flight existingFlight)
    {
        if (existingFlight == null)
            throw new ArgumentException("Flight not found", nameof(existingFlight));

        if (!string.IsNullOrEmpty(updateDto.FlightNumber) && string.IsNullOrWhiteSpace(updateDto.FlightNumber))
            throw new ArgumentException("Flight number cannot be empty", nameof(updateDto.FlightNumber));

        if (!string.IsNullOrEmpty(updateDto.Airline) && string.IsNullOrWhiteSpace(updateDto.Airline))
            throw new ArgumentException("Airline cannot be empty", nameof(updateDto.Airline));

        _logger.LogDebug("Flight validation passed for update: {FlightId}", existingFlight.Id);
    }

    /// <summary>
    /// Map CreateFlightDto to Flight entity
    /// </summary>
    public Flight MapCreateDtoToEntity(CreateFlightDto createDto)
    {
        var status = Enum.TryParse<FlightStatus>(createDto.Status, true, out var parsedStatus)
            ? parsedStatus
            : FlightStatus.Scheduled;

        return new Flight
        {
            FlightNumber = createDto.FlightNumber,
            Airline = createDto.Airline,
            Origin = createDto.Origin,
            Destination = createDto.Destination,
            ScheduledDeparture = createDto.ScheduledDeparture,
            ScheduledArrival = createDto.ScheduledArrival,
            Status = status,
            Gate = createDto.Gate,
            Terminal = createDto.Terminal,
            AircraftType = createDto.AircraftType,
            DelayMinutes = createDto.DelayMinutes,
            Remarks = createDto.Remarks,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Map UpdateFlightDto to existing Flight entity
    /// </summary>
    public Flight MapUpdateDtoToEntity(UpdateFlightDto updateDto, Flight existingFlight)
    {
        if (!string.IsNullOrEmpty(updateDto.FlightNumber))
            existingFlight.FlightNumber = updateDto.FlightNumber;

        if (!string.IsNullOrEmpty(updateDto.Airline))
            existingFlight.Airline = updateDto.Airline;

        if (!string.IsNullOrEmpty(updateDto.Origin))
            existingFlight.Origin = updateDto.Origin;

        if (!string.IsNullOrEmpty(updateDto.Destination))
            existingFlight.Destination = updateDto.Destination;

        if (updateDto.ScheduledDeparture.HasValue)
            existingFlight.ScheduledDeparture = updateDto.ScheduledDeparture.Value;

        if (updateDto.ScheduledArrival.HasValue)
            existingFlight.ScheduledArrival = updateDto.ScheduledArrival.Value;

        if (updateDto.ActualDeparture.HasValue)
            existingFlight.ActualDeparture = updateDto.ActualDeparture.Value;

        if (updateDto.ActualArrival.HasValue)
            existingFlight.ActualArrival = updateDto.ActualArrival.Value;

        if (!string.IsNullOrEmpty(updateDto.Status))
        {
            if (Enum.TryParse<FlightStatus>(updateDto.Status, true, out var status))
                existingFlight.Status = status;
        }

        if (updateDto.Gate != null)
            existingFlight.Gate = updateDto.Gate;

        if (updateDto.Terminal != null)
            existingFlight.Terminal = updateDto.Terminal;

        if (updateDto.AircraftType != null)
            existingFlight.AircraftType = updateDto.AircraftType;

        if (updateDto.DelayMinutes.HasValue)
            existingFlight.DelayMinutes = updateDto.DelayMinutes.Value;

        if (updateDto.Remarks != null)
            existingFlight.Remarks = updateDto.Remarks;

        existingFlight.UpdatedAt = DateTime.UtcNow;

        return existingFlight;
    }
}
