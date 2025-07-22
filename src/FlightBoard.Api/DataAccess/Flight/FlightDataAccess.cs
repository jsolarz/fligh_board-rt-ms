using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.DataAccess.Flight;

/// <summary>
/// Flight data access implementation following iDesign Method principles
/// DataAccess layer handles all database operations for flights
/// </summary>
public class FlightDataAccess : IFlightDataAccess
{
    private readonly FlightDbContext _context;
    private readonly ILogger<FlightDataAccess> _logger;

    public FlightDataAccess(FlightDbContext context, ILogger<FlightDataAccess> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<IQueryable<Models.Flight>> GetFlightsQueryAsync()
    {
        return Task.FromResult(_context.Flights.AsQueryable());
    }

    public async Task<Models.Flight?> GetFlightByIdAsync(int id)
    {
        try
        {
            return await _context.Flights.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flight with ID {FlightId}", id);
            throw;
        }
    }

    public async Task<Models.Flight> CreateFlightAsync(Models.Flight flight)
    {
        try
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return flight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight {FlightNumber}", flight.FlightNumber);
            throw;
        }
    }

    public async Task<Models.Flight> UpdateFlightAsync(Models.Flight flight)
    {
        try
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
            return flight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight with ID {FlightId}", flight.Id);
            throw;
        }
    }

    public async Task<bool> DeleteFlightAsync(int id)
    {
        try
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return false;

            flight.IsDeleted = true;
            flight.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight with ID {FlightId}", id);
            throw;
        }
    }

    public async Task<int> GetTotalFlightsCountAsync(IQueryable<Models.Flight> query)
    {
        return await query.CountAsync();
    }

    public async Task<List<Models.Flight>> GetPagedFlightsAsync(IQueryable<Models.Flight> query, int page, int pageSize)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
