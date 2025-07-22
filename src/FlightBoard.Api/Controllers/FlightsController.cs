using Microsoft.AspNetCore.Mvc;
using FlightBoard.Api.DTOs;
using FlightBoard.Api.Contract.Flight;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// API controller for flight operations following iDesign Method principles
/// Controllers call Managers to orchestrate use cases
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FlightsController : ControllerBase
{
    private readonly IFlightManager _flightManager;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IFlightManager flightManager, ILogger<FlightsController> logger)
    {
        _flightManager = flightManager;
        _logger = logger;
    }

    /// <summary>
    /// Get all flights with optional filtering and pagination
    /// </summary>
    /// <param name="searchDto">Search and filter parameters</param>
    /// <returns>Paginated list of flights</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<FlightDto>>> GetFlights([FromQuery] FlightSearchDto searchDto)
    {
        try
        {
            var result = await _flightManager.GetFlightsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights");
            return StatusCode(500, "An error occurred while retrieving flights");
        }
    }

    /// <summary>
    /// Get flight by ID
    /// </summary>
    /// <param name="id">Flight ID</param>
    /// <returns>Flight details</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FlightDto>> GetFlight(int id)
    {
        try
        {
            var flight = await _flightManager.GetFlightByIdAsync(id);
            if (flight == null)
                return NotFound($"Flight with ID {id} not found");

            return Ok(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flight with ID {FlightId}", id);
            return StatusCode(500, "An error occurred while retrieving the flight");
        }
    }

    /// <summary>
    /// Create a new flight
    /// </summary>
    /// <param name="createDto">Flight creation data</param>
    /// <returns>Created flight</returns>
    [HttpPost]
    public async Task<ActionResult<FlightDto>> CreateFlight([FromBody] CreateFlightDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var flight = await _flightManager.CreateFlightAsync(createDto);
            return CreatedAtAction(nameof(GetFlight), new { id = flight.Id }, flight);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating flight");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight");
            return StatusCode(500, "An error occurred while creating the flight");
        }
    }

    /// <summary>
    /// Update an existing flight
    /// </summary>
    /// <param name="id">Flight ID</param>
    /// <param name="updateDto">Flight update data</param>
    /// <returns>Updated flight</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FlightDto>> UpdateFlight(int id, [FromBody] UpdateFlightDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var flight = await _flightManager.UpdateFlightAsync(id, updateDto);
            return Ok(flight);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating flight with ID {FlightId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight with ID {FlightId}", id);
            return StatusCode(500, "An error occurred while updating the flight");
        }
    }

    /// <summary>
    /// Delete a flight
    /// </summary>
    /// <param name="id">Flight ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteFlight(int id)
    {
        try
        {
            var result = await _flightManager.DeleteFlightAsync(id);
            if (!result)
                return NotFound($"Flight with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight with ID {FlightId}", id);
            return StatusCode(500, "An error occurred while deleting the flight");
        }
    }

    /// <summary>
    /// Update flight status
    /// </summary>
    /// <param name="id">Flight ID</param>
    /// <param name="statusRequest">Status update request</param>
    /// <returns>Updated flight</returns>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<FlightDto>> UpdateFlightStatus(int id, [FromBody] UpdateStatusRequest statusRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var flight = await _flightManager.UpdateFlightStatusAsync(id, statusRequest.Status);
            return Ok(flight);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status update for flight with ID {FlightId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight status for ID {FlightId}", id);
            return StatusCode(500, "An error occurred while updating the flight status");
        }
    }

    /// <summary>
    /// Search flights with filtering - legacy endpoint maintained for compatibility
    /// </summary>
    /// <param name="searchDto">Search and filter parameters</param>
    /// <returns>Paginated list of matching flights</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<FlightDto>>> SearchFlights([FromQuery] FlightSearchDto searchDto)
    {
        try
        {
            _logger.LogInformation("Searching flights with filters: FlightNumber={FlightNumber}, Airline={Airline}, Status={Status}",
                searchDto.FlightNumber, searchDto.Airline, searchDto.Status);

            // Use the same method as the main GET endpoint for consistency
            var result = await _flightManager.GetFlightsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights");
            return StatusCode(500, "An error occurred while searching flights");
        }
    }

    /// <summary>
    /// Get departure flights with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>Paginated departure flights</returns>
    [HttpGet("departures")]
    public async Task<ActionResult<PagedResponse<FlightDto>>> GetDepartures([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var searchDto = new FlightSearchDto { Type = "Departure", Page = page, PageSize = pageSize };
            var result = await _flightManager.GetFlightsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departure flights");
            return StatusCode(500, "An error occurred while retrieving departure flights");
        }
    }

    /// <summary>
    /// Get arrival flights with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>Paginated arrival flights</returns>
    [HttpGet("arrivals")]
    public async Task<ActionResult<PagedResponse<FlightDto>>> GetArrivals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var searchDto = new FlightSearchDto { Type = "Arrival", Page = page, PageSize = pageSize };
            var result = await _flightManager.GetFlightsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving arrival flights");
            return StatusCode(500, "An error occurred while retrieving arrival flights");
        }
    }

    /// <summary>
    /// Get flights by departure date (optimized with caching)
    /// </summary>
    /// <param name="date">Departure date (YYYY-MM-DD format)</param>
    /// <returns>List of flights departing on the specified date</returns>
    [HttpGet("departures/{date:datetime}")]
    public async Task<ActionResult<List<FlightDto>>> GetFlightsByDeparture(DateTime date)
    {
        try
        {
            var result = await _flightManager.GetFlightsByDepartureDateAsync(date);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights by departure date {Date}", date);
            return StatusCode(500, "An error occurred while retrieving departure flights");
        }
    }

    /// <summary>
    /// Get flights by arrival date (optimized with caching)
    /// </summary>
    /// <param name="date">Arrival date (YYYY-MM-DD format)</param>
    /// <returns>List of flights arriving on the specified date</returns>
    [HttpGet("arrivals/{date:datetime}")]
    public async Task<ActionResult<List<FlightDto>>> GetFlightsByArrival(DateTime date)
    {
        try
        {
            var result = await _flightManager.GetFlightsByArrivalDateAsync(date);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights by arrival date {Date}", date);
            return StatusCode(500, "An error occurred while retrieving arrival flights");
        }
    }

    /// <summary>
    /// Get flights by status (optimized with caching)
    /// </summary>
    /// <param name="status">Flight status (scheduled, delayed, boarding, etc.)</param>
    /// <returns>List of flights with the specified status</returns>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<FlightDto>>> GetFlightsByStatus(string status)
    {
        try
        {
            var result = await _flightManager.GetFlightsByStatusAsync(status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights by status {Status}", status);
            return StatusCode(500, "An error occurred while retrieving flights by status");
        }
    }
}
