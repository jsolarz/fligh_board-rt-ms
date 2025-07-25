using FlightBoard.Api.Core.Entities;

namespace FlightBoard.Api.DataAccess.Flight;

/// <summary>
/// Interface for flight data access operations following iDesign Method principles
/// DataAccess layer handles all database operations and data persistence
/// </summary>
public interface IFlightDataAccess
{
    Task<IQueryable<Core.Entities.Flight>> GetFlightsQueryAsync();
    Task<Core.Entities.Flight?> GetFlightByIdAsync(int id);
    Task<Core.Entities.Flight> CreateFlightAsync(Core.Entities.Flight flight);
    Task<Core.Entities.Flight> UpdateFlightAsync(Core.Entities.Flight flight);
    Task<bool> DeleteFlightAsync(int id);
    Task<int> GetTotalFlightsCountAsync(IQueryable<Core.Entities.Flight> query);
    Task<List<Core.Entities.Flight>> GetPagedFlightsAsync(IQueryable<Core.Entities.Flight> query, int page, int pageSize);
    Task SaveChangesAsync();
    
    // Cached query methods for performance optimization
    Task<List<Core.Entities.Flight>> GetFlightsByDepartureDateAsync(DateTime date);
    Task<List<Core.Entities.Flight>> GetFlightsByArrivalDateAsync(DateTime date);
    Task<List<Core.Entities.Flight>> GetFlightsByStatusAsync(string status);
}
