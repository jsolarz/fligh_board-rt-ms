using Microsoft.AspNetCore.Mvc;
using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;
using FlightBoard.Api.Services;

namespace FlightBoard.Api.Controllers;

/// <summary>
/// API controller for flight operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FlightsController : ControllerBase
{
    private readonly FlightService _flightService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(FlightService flightService, ILogger<FlightsController> logger)
    {
        _flightService = flightService;
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
            var result = await _flightService.GetFlightsAsync(searchDto);
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
            var flight = await _flightService.GetFlightByIdAsync(id);
            if (flight == null)
                return NotFound($"Flight with ID {id} not found");

            return Ok(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flight {FlightId}", id);
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

            var flight = await _flightService.CreateFlightAsync(createDto);
            return CreatedAtAction(nameof(GetFlight), new { id = flight.Id }, flight);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating flight: {Error}", ex.Message);
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

            var flight = await _flightService.UpdateFlightAsync(id, updateDto);
            if (flight == null)
                return NotFound($"Flight with ID {id} not found");

            return Ok(flight);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error updating flight {FlightId}: {Error}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight {FlightId}", id);
            return StatusCode(500, "An error occurred while updating the flight");
        }
    }

    /// <summary>
    /// Delete a flight (soft delete)
    /// </summary>
    /// <param name="id">Flight ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteFlight(int id)
    {
        try
        {
            var result = await _flightService.DeleteFlightAsync(id);
            if (!result)
                return NotFound($"Flight with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight {FlightId}", id);
            return StatusCode(500, "An error occurred while deleting the flight");
        }
    }

    /// <summary>
    /// Get departures with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated departure flights</returns>
    [HttpGet("departures")]
    public async Task<ActionResult<PagedResponse<FlightDto>>> GetDepartures([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _flightService.GetFlightsByTypeAsync(FlightType.Departure, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departures");
            return StatusCode(500, "An error occurred while retrieving departures");
        }
    }

    /// <summary>
    /// Get arrivals with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated arrival flights</returns>
    [HttpGet("arrivals")]
    public async Task<ActionResult<PagedResponse<FlightDto>>> GetArrivals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _flightService.GetFlightsByTypeAsync(FlightType.Arrival, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving arrivals");
            return StatusCode(500, "An error occurred while retrieving arrivals");
        }
    }

    /// <summary>
    /// Get currently active flights
    /// </summary>
    /// <returns>List of active flights</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<FlightDto>>> GetActiveFlights()
    {
        try
        {
            var flights = await _flightService.GetActiveFlightsAsync();
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active flights");
            return StatusCode(500, "An error occurred while retrieving active flights");
        }
    }

    /// <summary>
    /// Get delayed flights
    /// </summary>
    /// <returns>List of delayed flights</returns>
    [HttpGet("delayed")]
    public async Task<ActionResult<IEnumerable<FlightDto>>> GetDelayedFlights()
    {
        try
        {
            var flights = await _flightService.GetDelayedFlightsAsync();
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delayed flights");
            return StatusCode(500, "An error occurred while retrieving delayed flights");
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
            if (!Enum.TryParse<FlightStatus>(statusRequest.Status, out var status))
                return BadRequest("Invalid flight status");

            var flight = await _flightService.UpdateFlightStatusAsync(id, status, statusRequest.Remarks);
            if (flight == null)
                return NotFound($"Flight with ID {id} not found");

            return Ok(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight status for {FlightId}", id);
            return StatusCode(500, "An error occurred while updating flight status");
        }
    }

    /// <summary>
    /// Search flights by status and/or destination (required by objectives.md)
    /// </summary>
    /// <param name="status">Filter by flight status (optional)</param>
    /// <param name="destination">Filter by destination airport code (optional)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Paginated and filtered list of flights</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<FlightDto>>> SearchFlights(
        [FromQuery] string? status = null,
        [FromQuery] string? destination = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Create search DTO with filter parameters
            var searchDto = new FlightSearchDto
            {
                Status = status,
                Destination = destination,
                Page = page,
                PageSize = pageSize
            };

            var result = await _flightService.GetFlightsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights with status: {Status}, destination: {Destination}",
                status, destination);
            return StatusCode(500, "An error occurred while searching flights");
        }
    }
}
