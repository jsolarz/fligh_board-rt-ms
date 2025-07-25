using FlightBoard.Api.Data;
using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;
using FlightBoard.Api.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace FlightBoard.Api.Services;

/// <summary>
/// Service for flight operations and business logic
/// </summary>
public class FlightService
{
    private readonly FlightDbContext _context;
    private readonly ILogger<FlightService> _logger; private readonly IHubContext<FlightHub> _flightHub;

    public FlightService(FlightDbContext context, ILogger<FlightService> logger, IHubContext<FlightHub> flightHub)
    {
        _context = context;
        _logger = logger;
        _flightHub = flightHub;
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
    /// Get all flights with optional filtering and pagination
    /// </summary>
    public async Task<PagedResponse<FlightDto>> GetFlightsAsync(FlightSearchDto searchDto)
    {
        var query = _context.Flights.AsQueryable();

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
            query = query.Where(f => f.Status.ToString() == searchDto.Status);

        if (!string.IsNullOrEmpty(searchDto.Type))
            query = query.Where(f => f.Type.ToString() == searchDto.Type);

        if (searchDto.FromDate.HasValue)
            query = query.Where(f => f.ScheduledDeparture >= searchDto.FromDate.Value);

        if (searchDto.ToDate.HasValue)
            query = query.Where(f => f.ScheduledDeparture <= searchDto.ToDate.Value);

        if (searchDto.IsDelayed.HasValue)
            query = query.Where(f => (f.DelayMinutes > 15) == searchDto.IsDelayed.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();        // Apply pagination
        var flights = await query
            .OrderBy(f => f.ScheduledDeparture)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        // Convert to DTOs with calculated status
        var flightDtos = FlightMappingService.ToDto(flights, flight =>
            CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture));

        return new PagedResponse<FlightDto>
        {
            Data = flightDtos,
            Page = searchDto.Page,
            PageSize = searchDto.PageSize,
            TotalCount = totalCount
        };
    }    /// <summary>
         /// Get flight by ID
         /// </summary>
    public async Task<FlightDto?> GetFlightByIdAsync(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null) return null;

        // Calculate real-time status
        var calculatedStatus = CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);
        return FlightMappingService.ToDto(flight, calculatedStatus);
    }

    /// <summary>
    /// Create a new flight
    /// </summary>
    public async Task<FlightDto> CreateFlightAsync(CreateFlightDto createDto)
    {
        // Validate flight data
        await ValidateFlightDataAsync(createDto);

        var flight = FlightMappingService.ToEntity(createDto);
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new flight {FlightNumber} with ID {Id}", flight.FlightNumber, flight.Id);

        // Calculate real-time status for the new flight
        var calculatedStatus = CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);
        var flightDto = FlightMappingService.ToDto(flight, calculatedStatus);

        // Send real-time notification to all connected clients (if available)
        if (_flightHub != null)
        {
            await _flightHub.Clients.All.SendAsync("FlightCreated", flightDto);
            await _flightHub.Clients.Group("AllFlights").SendAsync("FlightAdded", flightDto);

            // Send to specific flight type groups
            if (flight.Type == FlightType.Departure)
                await _flightHub.Clients.Group("Departures").SendAsync("FlightAdded", flightDto);
            else
                await _flightHub.Clients.Group("Arrivals").SendAsync("FlightAdded", flightDto);
        }

        return flightDto;
    }

    /// <summary>
    /// Update an existing flight
    /// </summary>
    public async Task<FlightDto?> UpdateFlightAsync(int id, UpdateFlightDto updateDto)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null)
            return null; var originalFlightNumber = flight.FlightNumber;
        FlightMappingService.UpdateEntity(flight, updateDto);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated flight {FlightNumber} (ID: {Id})", originalFlightNumber, flight.Id);

        // Calculate real-time status for the updated flight
        var calculatedStatus = CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);
        var flightDto = FlightMappingService.ToDto(flight, calculatedStatus);

        // Send real-time notification to all connected clients (if available)
        if (_flightHub != null)
        {
            await _flightHub.Clients.All.SendAsync("FlightUpdated", flightDto);
            await _flightHub.Clients.Group("AllFlights").SendAsync("FlightUpdated", flightDto);

            // Send to specific flight type groups
            if (flight.Type == FlightType.Departure)
                await _flightHub.Clients.Group("Departures").SendAsync("FlightUpdated", flightDto);
            else if (flight.Type == FlightType.Arrival)
                await _flightHub.Clients.Group("Arrivals").SendAsync("FlightUpdated", flightDto);
        }

        return flightDto;
    }

    /// <summary>
    /// Soft delete a flight
    /// </summary>
    public async Task<bool> DeleteFlightAsync(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null)
            return false;

        flight.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Soft deleted flight {FlightNumber} (ID: {Id})", flight.FlightNumber, flight.Id);

        return true;
    }    /// <summary>
         /// Get flights by type (Departure or Arrival) with server-side status calculation
         /// </summary>
    public async Task<PagedResponse<FlightDto>> GetFlightsByTypeAsync(FlightType type, int page = 1, int pageSize = 20)
    {
        var query = _context.Flights.Where(f => f.Type == type);

        var totalCount = await query.CountAsync();

        var flights = await query
            .OrderBy(f => f.ScheduledDeparture)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Convert to DTOs with calculated status
        var flightDtos = FlightMappingService.ToDto(flights, flight =>
            CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture));

        return new PagedResponse<FlightDto>
        {
            Data = flightDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Get current active flights (not cancelled, not completed)
    /// </summary>
    public async Task<IEnumerable<FlightDto>> GetActiveFlightsAsync()
    {
        var activeStatuses = new[] { FlightStatus.Scheduled, FlightStatus.Delayed, FlightStatus.Boarding, FlightStatus.Departed, FlightStatus.InFlight, FlightStatus.Landed }; var flights = await _context.Flights
            .Where(f => activeStatuses.Contains(f.Status))
            .OrderBy(f => f.ScheduledDeparture)
            .ToListAsync();

        // Convert to DTOs with calculated status
        return FlightMappingService.ToDto(flights, flight =>
            CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture));
    }

    /// <summary>
    /// Get delayed flights with server-side status calculation
    /// </summary>
    public async Task<IEnumerable<FlightDto>> GetDelayedFlightsAsync()
    {
        var flights = await _context.Flights
            .Where(f => f.DelayMinutes > 15)
            .OrderByDescending(f => f.DelayMinutes)
            .ToListAsync();

        // Convert to DTOs with calculated status
        return FlightMappingService.ToDto(flights, flight =>
            CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture));
    }

    /// <summary>
    /// Update flight status
    /// </summary>
    public async Task<FlightDto?> UpdateFlightStatusAsync(int id, FlightStatus status, string? remarks = null)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null)
            return null;

        var oldStatus = flight.Status;
        flight.Status = status;

        if (!string.IsNullOrEmpty(remarks))
            flight.Remarks = remarks;

        // Auto-set actual departure/arrival times based on status
        var now = DateTime.UtcNow;
        switch (status)
        {
            case FlightStatus.Departed when flight.ActualDeparture == null:
                flight.ActualDeparture = now;
                break;
            case FlightStatus.Arrived when flight.ActualArrival == null:
                flight.ActualArrival = now;
                break;
        }
        await _context.SaveChangesAsync(); _logger.LogInformation("Updated flight {FlightNumber} status from {OldStatus} to {NewStatus}",
            flight.FlightNumber, oldStatus, status);

        // Calculate real-time status (may override manual status with time-based calculation)
        var calculatedStatus = CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);
        var flightDto = FlightMappingService.ToDto(flight, calculatedStatus);

        // Send real-time status update notifications (if available)
        if (_flightHub != null)
        {
            await _flightHub.Clients.All.SendAsync("FlightStatusChanged", flightDto, oldStatus.ToString(), calculatedStatus.ToString());
            await _flightHub.Clients.Group("AllFlights").SendAsync("FlightUpdated", flightDto);

            // Send to specific flight type groups
            if (flight.Type == FlightType.Departure)
                await _flightHub.Clients.Group("Departures").SendAsync("FlightUpdated", flightDto);
            else if (flight.Type == FlightType.Arrival)
                await _flightHub.Clients.Group("Arrivals").SendAsync("FlightUpdated", flightDto);
        }

        return flightDto;
    }

    /// <summary>
    /// Validate flight data for business rules
    /// </summary>
    private async Task ValidateFlightDataAsync(CreateFlightDto dto)
    {
        var errors = new List<string>();

        // Check for duplicate flight number on the same date
        var existingFlight = await _context.Flights
            .Where(f => f.FlightNumber == dto.FlightNumber &&
                       f.ScheduledDeparture.Date == dto.ScheduledDeparture.Date)
            .FirstOrDefaultAsync();

        if (existingFlight != null)
            errors.Add($"Flight {dto.FlightNumber} already exists for {dto.ScheduledDeparture:yyyy-MM-dd}");

        // Validate arrival is after departure
        if (dto.ScheduledArrival <= dto.ScheduledDeparture)
            errors.Add("Scheduled arrival must be after scheduled departure");

        // Validate flight number format (airline code + number)
        if (dto.FlightNumber.Length < 3 || !char.IsLetter(dto.FlightNumber[0]))
            errors.Add("Invalid flight number format");

        if (errors.Any())
            throw new ValidationException(string.Join("; ", errors));
    }
}

/// <summary>
/// Custom validation exception
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
