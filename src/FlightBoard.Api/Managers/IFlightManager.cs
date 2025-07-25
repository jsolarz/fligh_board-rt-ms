using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.Managers;

/// <summary>
/// Internal interface for flight management operations following iDesign Method principles
/// This is the internal contract used within the Manager layer
/// </summary>
internal interface IFlightManagerInternal
{
    Task<PagedResponse<FlightDto>> GetFlightsAsync(FlightSearchDto searchDto);
    Task<FlightDto?> GetFlightByIdAsync(int id);
    Task<FlightDto> CreateFlightAsync(CreateFlightDto createDto);
    Task<FlightDto> UpdateFlightAsync(int id, UpdateFlightDto updateDto);
    Task<bool> DeleteFlightAsync(int id);
    Task<FlightDto> UpdateFlightStatusAsync(int id, string status);
}
