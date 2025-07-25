using FlightBoard.Api.Core.DTOs;

namespace FlightBoard.Api.Contract.Flight;

/// <summary>
/// Public contract for Flight Manager - Use case orchestration interface
/// Following iDesign Method: Only public manager contracts are in Contract namespace
/// </summary>
public interface IFlightManager
{
    // Flight Search and Queries - matches current implementation
    Task<PagedResponse<FlightDto>> GetFlightsAsync(FlightSearchDto searchDto);
    Task<FlightDto?> GetFlightByIdAsync(int id);

    // Flight CRUD Operations - matches current implementation
    Task<FlightDto> CreateFlightAsync(CreateFlightDto createDto);
    Task<FlightDto> UpdateFlightAsync(int id, UpdateFlightDto updateDto);
    Task<bool> DeleteFlightAsync(int id);

    // Status Management - matches current implementation
    Task<FlightDto> UpdateFlightStatusAsync(int id, string status);
    
    // Performance-optimized cached queries
    Task<List<FlightDto>> GetFlightsByDepartureDateAsync(DateTime date);
    Task<List<FlightDto>> GetFlightsByArrivalDateAsync(DateTime date);
    Task<List<FlightDto>> GetFlightsByStatusAsync(string status);
}
