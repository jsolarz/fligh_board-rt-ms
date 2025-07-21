using FlightBoard.Api.Data;
using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightBoard.Api.Services;

/// <summary>
/// Service for flight operations and business logic
/// </summary>
public class FlightService
{
    private readonly FlightDbContext _context;
    private readonly ILogger<FlightService> _logger;

    public FlightService(FlightDbContext context, ILogger<FlightService> logger)
    {
        _context = context;
        _logger = logger;
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
        var totalCount = await query.CountAsync();

        // Apply pagination
        var flights = await query
            .OrderBy(f => f.ScheduledDeparture)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        return FlightMappingService.ToPagedResponse(flights, searchDto.Page, searchDto.PageSize, totalCount);
    }

    /// <summary>
    /// Get flight by ID
    /// </summary>
    public async Task<FlightDto?> GetFlightByIdAsync(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        return flight != null ? FlightMappingService.ToDto(flight) : null;
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

        return FlightMappingService.ToDto(flight);
    }

    /// <summary>
    /// Update an existing flight
    /// </summary>
    public async Task<FlightDto?> UpdateFlightAsync(int id, UpdateFlightDto updateDto)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null)
            return null;

        var originalFlightNumber = flight.FlightNumber;

        FlightMappingService.UpdateEntity(flight, updateDto);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated flight {FlightNumber} (ID: {Id})", originalFlightNumber, flight.Id);

        return FlightMappingService.ToDto(flight);
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
    }

    /// <summary>
    /// Get flights by type (Departure or Arrival)
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

        return FlightMappingService.ToPagedResponse(flights, page, pageSize, totalCount);
    }

    /// <summary>
    /// Get current active flights (not cancelled, not completed)
    /// </summary>
    public async Task<IEnumerable<FlightDto>> GetActiveFlightsAsync()
    {
        var activeStatuses = new[] { FlightStatus.Scheduled, FlightStatus.Delayed, FlightStatus.Boarding, FlightStatus.Departed, FlightStatus.InFlight, FlightStatus.Landed };

        var flights = await _context.Flights
            .Where(f => activeStatuses.Contains(f.Status))
            .OrderBy(f => f.ScheduledDeparture)
            .ToListAsync();

        return FlightMappingService.ToDto(flights);
    }

    /// <summary>
    /// Get delayed flights
    /// </summary>
    public async Task<IEnumerable<FlightDto>> GetDelayedFlightsAsync()
    {
        var flights = await _context.Flights
            .Where(f => f.DelayMinutes > 15)
            .OrderByDescending(f => f.DelayMinutes)
            .ToListAsync();

        return FlightMappingService.ToDto(flights);
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

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated flight {FlightNumber} status from {OldStatus} to {NewStatus}",
            flight.FlightNumber, oldStatus, status);

        return FlightMappingService.ToDto(flight);
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
