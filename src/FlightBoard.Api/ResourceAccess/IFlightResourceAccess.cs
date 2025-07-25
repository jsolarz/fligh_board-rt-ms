using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.ResourceAccess;

/// <summary>
/// Interface for flight data access operations following iDesign Method principles
/// </summary>
public interface IFlightResourceAccess
{
    Task<IQueryable<Flight>> GetFlightsQueryAsync();
    Task<Flight?> GetFlightByIdAsync(int id);
    Task<Flight> CreateFlightAsync(Flight flight);
    Task<Flight> UpdateFlightAsync(Flight flight);
    Task<bool> DeleteFlightAsync(int id);
    Task<int> GetTotalFlightsCountAsync(IQueryable<Flight> query);
    Task<List<Flight>> GetPagedFlightsAsync(IQueryable<Flight> query, int page, int pageSize);
    Task SaveChangesAsync();
}
