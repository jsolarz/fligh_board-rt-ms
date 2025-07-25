using FlightBoard.Api.Core.DTOs;
using FlightBoard.Api.Core.Entities;
using FlightBoard.Api.Engines;
using FlightBoard.Api.DataAccess.Flight;
using FlightBoard.Api.iFX.CrossCutting.Notifications;
using FlightBoard.Api.iFX.Utilities.Mapping;
using FlightBoard.Api.Contract.Flight;

namespace FlightBoard.Api.Managers;

/// <summary>
/// Flight manager for orchestrating use cases following iDesign Method principles
/// Controls the flow of flight operations by coordinating engines and data access
/// Implements the public contract interface for external consumption
/// </summary>
public class FlightManager : IFlightManager
{
    private readonly IFlightDataAccess _flightDataAccess;
    private readonly IFlightEngine _flightEngine;
    private readonly INotificationEngine _notificationEngine;
    private readonly IFlightMappingUtility _mappingUtility;
    private readonly ILogger<FlightManager> _logger;

    public FlightManager(
        IFlightDataAccess flightDataAccess,
        IFlightEngine flightEngine,
        INotificationEngine notificationEngine,
        IFlightMappingUtility mappingUtility,
        ILogger<FlightManager> logger)
    {
        _flightDataAccess = flightDataAccess;
        _flightEngine = flightEngine;
        _notificationEngine = notificationEngine;
        _mappingUtility = mappingUtility;
        _logger = logger;
    }

    /// <summary>
    /// Get flights with search filters and pagination
    /// </summary>
    public async Task<PagedResponse<FlightDto>> GetFlightsAsync(FlightSearchDto searchDto)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetFlights",
            ["SearchCriteria"] = new
            {
                searchDto.FlightNumber,
                searchDto.Destination,
                searchDto.Status,
                searchDto.Airline,
                searchDto.Origin,
                searchDto.IsDelayed,
                searchDto.Page,
                searchDto.PageSize
            }
        });

        try
        {
            _logger.LogInformation("Getting flights with search criteria: {SearchCriteria}",
                new { searchDto.FlightNumber, searchDto.Destination, searchDto.Status, searchDto.Airline });

            // Get base query from resource access
            var query = await _flightDataAccess.GetFlightsQueryAsync();

            // Apply business logic filters via engine
            var filteredQuery = _flightEngine.ApplySearchFilters(query, searchDto);

            // Get total count for pagination
            var totalCount = await _flightDataAccess.GetTotalFlightsCountAsync(filteredQuery);

            // Get paged results
            var flights = await _flightDataAccess.GetPagedFlightsAsync(filteredQuery, searchDto.Page, searchDto.PageSize);

            // Map to DTOs with calculated status
            var flightDtos = new List<FlightDto>();
            foreach (var flight in flights)
            {
                var calculatedStatus = _flightEngine.CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);
                var flightDto = _mappingUtility.MapToDto(flight, calculatedStatus);
                flightDtos.Add(flightDto);
            }

            return new PagedResponse<FlightDto>
            {
                Data = flightDtos,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights");
            throw;
        }
    }

    /// <summary>
    /// Get a single flight by ID
    /// </summary>
    public async Task<FlightDto?> GetFlightByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting flight by ID: {FlightId}", id);

            var flight = await _flightDataAccess.GetFlightByIdAsync(id);
            if (flight == null)
                return null;

            // Calculate current status using business engine
            var calculatedStatus = _flightEngine.CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);

            return _mappingUtility.MapToDto(flight, calculatedStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight by ID: {FlightId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new flight
    /// </summary>
    public async Task<FlightDto> CreateFlightAsync(CreateFlightDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new flight: {FlightNumber}", createDto.FlightNumber);

            // Validate using business engine
            _flightEngine.ValidateFlightForCreation(createDto);

            // Map DTO to entity using business engine
            var flight = _flightEngine.MapCreateDtoToEntity(createDto);

            // Calculate initial status
            var calculatedStatus = _flightEngine.CalculateFlightStatus(flight.ScheduledDeparture, flight.ActualDeparture);
            flight.Status = calculatedStatus;

            // Persist via resource access
            var createdFlight = await _flightDataAccess.CreateFlightAsync(flight);

            // Map to DTO
            var flightDto = _mappingUtility.MapToDto(createdFlight, calculatedStatus);

            // Notify via cross-cutting concern
            await _notificationEngine.NotifyFlightCreatedAsync(flightDto);

            _logger.LogInformation("Successfully created flight: {FlightNumber} with ID: {FlightId}",
                createDto.FlightNumber, createdFlight.Id);

            return flightDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight: {FlightNumber}", createDto.FlightNumber);
            throw;
        }
    }

    /// <summary>
    /// Update an existing flight
    /// </summary>
    public async Task<FlightDto> UpdateFlightAsync(int id, UpdateFlightDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating flight: {FlightId}", id);

            // Get existing flight via data access
            var existingFlight = await _flightDataAccess.GetFlightByIdAsync(id);
            if (existingFlight == null)
                throw new ArgumentException($"Flight with ID {id} not found");

            // Store previous state for notifications
            var previousStatus = _flightEngine.CalculateFlightStatus(existingFlight.ScheduledDeparture, existingFlight.ActualDeparture);
            var previousDto = _mappingUtility.MapToDto(existingFlight, previousStatus);

            // Validate using business engine
            _flightEngine.ValidateFlightForUpdate(updateDto, existingFlight);

            // Apply updates using business engine
            var updatedFlight = _flightEngine.MapUpdateDtoToEntity(updateDto, existingFlight);

            // Calculate new status
            var calculatedStatus = _flightEngine.CalculateFlightStatus(updatedFlight.ScheduledDeparture, updatedFlight.ActualDeparture);
            updatedFlight.Status = calculatedStatus;

            // Persist via data access
            var savedFlight = await _flightDataAccess.UpdateFlightAsync(updatedFlight);

            // Map to DTO
            var flightDto = _mappingUtility.MapToDto(savedFlight, calculatedStatus);

            // Notify via cross-cutting concern
            await _notificationEngine.NotifyFlightUpdatedAsync(flightDto, previousDto);

            _logger.LogInformation("Successfully updated flight: {FlightId}", id);

            return flightDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight: {FlightId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a flight (soft delete)
    /// </summary>
    public async Task<bool> DeleteFlightAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting flight: {FlightId}", id);

            var deleted = await _flightDataAccess.DeleteFlightAsync(id);
            if (deleted)
            {
                // Notify via cross-cutting concern
                await _notificationEngine.NotifyFlightDeletedAsync(id);
                _logger.LogInformation("Successfully deleted flight: {FlightId}", id);
            }

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight: {FlightId}", id);
            throw;
        }
    }

    /// <summary>
    /// Update flight status specifically
    /// </summary>
    public async Task<FlightDto> UpdateFlightStatusAsync(int id, string status)
    {
        try
        {
            _logger.LogInformation("Updating flight status: {FlightId} to {Status}", id, status);

            // Get existing flight
            var existingFlight = await _flightDataAccess.GetFlightByIdAsync(id);
            if (existingFlight == null)
                throw new ArgumentException($"Flight with ID {id} not found");

            var oldStatus = existingFlight.Status.ToString();

            // Parse and validate status
            if (!Enum.TryParse<FlightStatus>(status, true, out var newStatus))
                throw new ArgumentException($"Invalid flight status: {status}");

            // Update status
            existingFlight.Status = newStatus;
            existingFlight.UpdatedAt = DateTime.UtcNow;

            // Persist via data access
            var updatedFlight = await _flightDataAccess.UpdateFlightAsync(existingFlight);

            // Map to DTO
            var flightDto = _mappingUtility.MapToDto(updatedFlight, newStatus);

            // Notify status change via cross-cutting concern
            await _notificationEngine.NotifyFlightStatusChangedAsync(id, oldStatus, status);

            _logger.LogInformation("Successfully updated flight status: {FlightId} to {Status}", id, status);

            return flightDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight status: {FlightId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get flights by departure date (cached)
    /// </summary>
    public async Task<List<FlightDto>> GetFlightsByDepartureDateAsync(DateTime date)
    {
        try
        {
            _logger.LogInformation("Getting flights by departure date {Date}", date.ToString("yyyy-MM-dd"));

            var flights = await _flightDataAccess.GetFlightsByDepartureDateAsync(date);
            return flights.Select(_mappingUtility.MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FlightManager.GetFlightsByDepartureDateAsync for date {Date}", date);
            throw;
        }
    }

    /// <summary>
    /// Get flights by arrival date (cached)
    /// </summary>
    public async Task<List<FlightDto>> GetFlightsByArrivalDateAsync(DateTime date)
    {
        try
        {
            _logger.LogInformation("Getting flights by arrival date {Date}", date.ToString("yyyy-MM-dd"));

            var flights = await _flightDataAccess.GetFlightsByArrivalDateAsync(date);
            return flights.Select(_mappingUtility.MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FlightManager.GetFlightsByArrivalDateAsync for date {Date}", date);
            throw;
        }
    }

    /// <summary>
    /// Get flights by status (cached)
    /// </summary>
    public async Task<List<FlightDto>> GetFlightsByStatusAsync(string status)
    {
        try
        {
            _logger.LogInformation("Getting flights by status {Status}", status);

            var flights = await _flightDataAccess.GetFlightsByStatusAsync(status);
            return flights.Select(_mappingUtility.MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FlightManager.GetFlightsByStatusAsync for status {Status}", status);
            throw;
        }
    }
}
