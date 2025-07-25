using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.iFX.Utility;

namespace FlightBoard.Api.DataAccess.Flight;

/// <summary>
/// Flight data access implementation following iDesign Method principles
/// DataAccess layer handles all database operations for flights with caching
/// </summary>
public class FlightDataAccess : IFlightDataAccess
{
    private readonly FlightDbContext _context;
    private readonly ILogger<FlightDataAccess> _logger;
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    public FlightDataAccess(FlightDbContext context, ILogger<FlightDataAccess> logger, ICacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    public Task<IQueryable<Models.Flight>> GetFlightsQueryAsync()
    {
        return Task.FromResult(_context.Flights.AsQueryable());
    }

    public async Task<Models.Flight?> GetFlightByIdAsync(int id)
    {
        try
        {
            var cacheKey = CacheKeys.Format(CacheKeys.FLIGHT_DETAIL, id);
            var cachedFlight = await _cacheService.GetAsync<Models.Flight>(cacheKey);

            if (cachedFlight != null)
            {
                _logger.LogDebug("Flight {FlightId} retrieved from cache", id);
                return cachedFlight;
            }

            var flight = await _context.Flights.FindAsync(id);

            if (flight != null)
            {
                await _cacheService.SetAsync(cacheKey, flight, DefaultCacheDuration);
                _logger.LogDebug("Flight {FlightId} cached for {Duration} minutes", id, DefaultCacheDuration.TotalMinutes);
            }

            return flight;
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

            // Invalidate related caches
            await InvalidateFlightCaches();

            _logger.LogDebug("Created flight {FlightNumber} and invalidated caches", flight.FlightNumber);
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

            // Remove specific flight from cache
            var cacheKey = CacheKeys.Format(CacheKeys.FLIGHT_DETAIL, flight.Id);
            await _cacheService.RemoveAsync(cacheKey);

            // Invalidate related caches
            await InvalidateFlightCaches();

            _logger.LogDebug("Updated flight {FlightId} and invalidated caches", flight.Id);
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

            // Remove specific flight from cache
            var cacheKey = CacheKeys.Format(CacheKeys.FLIGHT_DETAIL, id);
            await _cacheService.RemoveAsync(cacheKey);

            // Invalidate related caches
            await InvalidateFlightCaches();

            _logger.LogDebug("Deleted flight {FlightId} and invalidated caches", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight with ID {FlightId}", id);
            throw;
        }
    }

    /// <summary>
    /// Invalidates flight-related caches when data changes
    /// </summary>
    private async Task InvalidateFlightCaches()
    {
        await _cacheService.RemoveByPatternAsync(CacheKeys.GetPattern("flights"));
        _logger.LogDebug("Invalidated all flight-related caches");
    }

    public async Task<int> GetTotalFlightsCountAsync(IQueryable<Models.Flight> query)
    {
        return await query.CountAsync();
    }

    public async Task<List<Models.Flight>> GetPagedFlightsAsync(IQueryable<Models.Flight> query, int page, int pageSize)
    {
        return await query
            .OrderBy(f => f.ScheduledDeparture) // Add default ordering for pagination
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets flights departing on a specific date with caching
    /// </summary>
    public async Task<List<Models.Flight>> GetFlightsByDepartureDateAsync(DateTime date)
    {
        try
        {
            var dateKey = date.ToString("yyyy-MM-dd");
            var cacheKey = CacheKeys.Format(CacheKeys.FLIGHTS_DEPARTURE, dateKey);
            var cachedFlights = await _cacheService.GetAsync<List<Models.Flight>>(cacheKey);

            if (cachedFlights != null)
            {
                _logger.LogDebug("Departure flights for {Date} retrieved from cache", dateKey);
                return cachedFlights;
            }

            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var flights = await _context.Flights
                .Where(f => !f.IsDeleted && f.ScheduledDeparture >= startDate && f.ScheduledDeparture < endDate)
                .OrderBy(f => f.ScheduledDeparture)
                .ToListAsync();

            await _cacheService.SetAsync(cacheKey, flights, DefaultCacheDuration);
            _logger.LogDebug("Cached {Count} departure flights for {Date}", flights.Count, dateKey);

            return flights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights by departure date {Date}", date);
            throw;
        }
    }

    /// <summary>
    /// Gets flights arriving on a specific date with caching
    /// </summary>
    public async Task<List<Models.Flight>> GetFlightsByArrivalDateAsync(DateTime date)
    {
        try
        {
            var dateKey = date.ToString("yyyy-MM-dd");
            var cacheKey = CacheKeys.Format(CacheKeys.FLIGHTS_ARRIVAL, dateKey);
            var cachedFlights = await _cacheService.GetAsync<List<Models.Flight>>(cacheKey);

            if (cachedFlights != null)
            {
                _logger.LogDebug("Arrival flights for {Date} retrieved from cache", dateKey);
                return cachedFlights;
            }

            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var flights = await _context.Flights
                .Where(f => !f.IsDeleted && f.ScheduledArrival >= startDate && f.ScheduledArrival < endDate)
                .OrderBy(f => f.ScheduledArrival)
                .ToListAsync();

            await _cacheService.SetAsync(cacheKey, flights, DefaultCacheDuration);
            _logger.LogDebug("Cached {Count} arrival flights for {Date}", flights.Count, dateKey);

            return flights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights by arrival date {Date}", date);
            throw;
        }
    }

    /// <summary>
    /// Gets flights by status with caching
    /// </summary>
    public async Task<List<Models.Flight>> GetFlightsByStatusAsync(string status)
    {
        try
        {
            var cacheKey = CacheKeys.Format(CacheKeys.FLIGHTS_STATUS, status);
            var cachedFlights = await _cacheService.GetAsync<List<Models.Flight>>(cacheKey);

            if (cachedFlights != null)
            {
                _logger.LogDebug("Flights with status {Status} retrieved from cache", status);
                return cachedFlights;
            }

            // Convert string to enum
            if (!Enum.TryParse<Models.FlightStatus>(status, true, out var flightStatus))
            {
                _logger.LogWarning("Invalid flight status provided: {Status}", status);
                return new List<Models.Flight>();
            }

            var flights = await _context.Flights
                .Where(f => !f.IsDeleted && f.Status == flightStatus)
                .OrderBy(f => f.ScheduledDeparture)
                .ToListAsync();

            await _cacheService.SetAsync(cacheKey, flights, DefaultCacheDuration);
            _logger.LogDebug("Cached {Count} flights with status {Status}", flights.Count, status);

            return flights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights by status {Status}", status);
            throw;
        }
    }
}
