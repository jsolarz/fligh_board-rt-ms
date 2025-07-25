using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.DataAccess.Flight;

/// <summary>
/// Interface for flight data access operations following iDesign Method principles
/// DataAccess layer handles all database operations and data persistence
/// </summary>
public interface IFlightDataAccess
{
    Task<IQueryable<Models.Flight>> GetFlightsQueryAsync();
    Task<Models.Flight?> GetFlightByIdAsync(int id);
    Task<Models.Flight> CreateFlightAsync(Models.Flight flight);
    Task<Models.Flight> UpdateFlightAsync(Models.Flight flight);
    Task<bool> DeleteFlightAsync(int id);
    Task<int> GetTotalFlightsCountAsync(IQueryable<Models.Flight> query);
    Task<List<Models.Flight>> GetPagedFlightsAsync(IQueryable<Models.Flight> query, int page, int pageSize);
    Task SaveChangesAsync();
    
    // Cached query methods for performance optimization
    Task<List<Models.Flight>> GetFlightsByDepartureDateAsync(DateTime date);
    Task<List<Models.Flight>> GetFlightsByArrivalDateAsync(DateTime date);
    Task<List<Models.Flight>> GetFlightsByStatusAsync(string status);
}
