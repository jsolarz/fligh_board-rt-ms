using FlightBoard.Api.Contract.Flight;
using FlightBoard.Api.iFX.Contract.Service;
using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.Manager;

/// <summary>
/// Cached flight manager with high-performance caching layer
/// Implements cache-aside pattern for optimal performance
/// Provides cache invalidation on data modifications
/// </summary>
public class CachedFlightManager : IFlightManager
{
    private readonly IFlightManager _baseManager;
    private readonly ICacheService _cacheService;
    private readonly IPerformanceService _performanceService;
    private readonly ILogger<CachedFlightManager> _logger;

    // Cache keys
    private const string FLIGHTS_SEARCH_KEY = "flights:search:{0}";
    private const string FLIGHTS_BY_STATUS_KEY = "flights:status:{0}";
    private const string FLIGHTS_BY_DATE_KEY = "flights:date:{0}:{1}"; // {type}:{date}
    private const string FLIGHT_KEY = "flight:{0}";

    // Cache durations
    private static readonly TimeSpan FlightCacheExpiration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan SearchCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ListCacheExpiration = TimeSpan.FromMinutes(10);

    public CachedFlightManager(
        IFlightManager baseManager,
        ICacheService cacheService,
        IPerformanceService performanceService,
        ILogger<CachedFlightManager> logger)
    {
        _baseManager = baseManager;
        _cacheService = cacheService;
        _performanceService = performanceService;
        _logger = logger;
    }

    public async Task<PagedResponse<FlightDto>> GetFlightsAsync(FlightSearchDto searchDto)
    {
        using var tracker = _performanceService.TrackOperation("GetFlights");
        
        var cacheKey = string.Format(FLIGHTS_SEARCH_KEY, 
            $"{searchDto.Page}:{searchDto.PageSize}:{searchDto.FlightNumber}:{searchDto.Status}:{searchDto.Type}:{searchDto.Airline}");
        
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Cache miss for flight search, fetching from database");
                return await _baseManager.GetFlightsAsync(searchDto);
            },
            SearchCacheExpiration) ?? new PagedResponse<FlightDto> 
            { 
                Data = new List<FlightDto>(), 
                Page = searchDto.Page, 
                PageSize = searchDto.PageSize, 
                TotalCount = 0
            };
    }

    public async Task<FlightDto?> GetFlightByIdAsync(int id)
    {
        using var tracker = _performanceService.TrackOperation("GetFlightById");
        
        var cacheKey = string.Format(FLIGHT_KEY, id);
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Cache miss for flight {FlightId}, fetching from database", id);
                return await _baseManager.GetFlightByIdAsync(id);
            },
            FlightCacheExpiration);
    }

    public async Task<List<FlightDto>> GetFlightsByStatusAsync(string status)
    {
        using var tracker = _performanceService.TrackOperation("GetFlightsByStatus");
        
        var cacheKey = string.Format(FLIGHTS_BY_STATUS_KEY, status.ToLower());
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Cache miss for flights with status {Status}, fetching from database", status);
                return await _baseManager.GetFlightsByStatusAsync(status);
            },
            ListCacheExpiration) ?? new List<FlightDto>();
    }

    public async Task<List<FlightDto>> GetFlightsByDepartureDateAsync(DateTime date)
    {
        using var tracker = _performanceService.TrackOperation("GetFlightsByDepartureDate");
        
        var cacheKey = string.Format(FLIGHTS_BY_DATE_KEY, "departure", date.ToString("yyyy-MM-dd"));
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Cache miss for flights on departure date {Date}, fetching from database", date);
                return await _baseManager.GetFlightsByDepartureDateAsync(date);
            },
            ListCacheExpiration) ?? new List<FlightDto>();
    }

    public async Task<List<FlightDto>> GetFlightsByArrivalDateAsync(DateTime date)
    {
        using var tracker = _performanceService.TrackOperation("GetFlightsByArrivalDate");
        
        var cacheKey = string.Format(FLIGHTS_BY_DATE_KEY, "arrival", date.ToString("yyyy-MM-dd"));
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Cache miss for flights on arrival date {Date}, fetching from database", date);
                return await _baseManager.GetFlightsByArrivalDateAsync(date);
            },
            ListCacheExpiration) ?? new List<FlightDto>();
    }

    public async Task<FlightDto> CreateFlightAsync(CreateFlightDto createDto)
    {
        using var tracker = _performanceService.TrackOperation("CreateFlight");
        
        var result = await _baseManager.CreateFlightAsync(createDto);
        
        // Invalidate relevant caches
        await InvalidateFlightCaches();
        
        _logger.LogInformation("Flight created: {FlightId}, cache invalidated", result.Id);
        _performanceService.TrackEvent("FlightCreated");
        
        return result;
    }

    public async Task<FlightDto> UpdateFlightAsync(int id, UpdateFlightDto updateDto)
    {
        using var tracker = _performanceService.TrackOperation("UpdateFlight");
        
        var result = await _baseManager.UpdateFlightAsync(id, updateDto);
        
        // Invalidate specific flight and related caches
        await InvalidateFlightCaches();
        await _cacheService.RemoveAsync(string.Format(FLIGHT_KEY, id));
        
        _logger.LogInformation("Flight updated: {FlightId}, cache invalidated", id);
        _performanceService.TrackEvent("FlightUpdated");
        
        return result;
    }

    public async Task<bool> DeleteFlightAsync(int id)
    {
        using var tracker = _performanceService.TrackOperation("DeleteFlight");
        
        var result = await _baseManager.DeleteFlightAsync(id);
        
        // Invalidate all flight-related caches
        await InvalidateFlightCaches();
        await _cacheService.RemoveAsync(string.Format(FLIGHT_KEY, id));
        
        _logger.LogInformation("Flight deleted: {FlightId}, cache invalidated", id);
        _performanceService.TrackEvent("FlightDeleted");
        
        return result;
    }

    public async Task<FlightDto> UpdateFlightStatusAsync(int id, string status)
    {
        using var tracker = _performanceService.TrackOperation("UpdateFlightStatus");
        
        var result = await _baseManager.UpdateFlightStatusAsync(id, status);
        
        // Invalidate specific flight and status-related caches
        await InvalidateFlightCaches();
        await _cacheService.RemoveAsync(string.Format(FLIGHT_KEY, id));
        
        _logger.LogInformation("Flight status updated: {FlightId} to {Status}, cache invalidated", id, status);
        _performanceService.TrackEvent("FlightStatusUpdated");
        
        return result;
    }

    private async Task InvalidateFlightCaches()
    {
        // Remove status-based caches (common statuses)
        var commonStatuses = new[] { "scheduled", "boarding", "departed", "arrived", "delayed", "cancelled" };
        foreach (var status in commonStatuses)
        {
            await _cacheService.RemoveAsync(string.Format(FLIGHTS_BY_STATUS_KEY, status));
        }
        
        // Remove date-based caches (last 7 days)
        var today = DateTime.Today;
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(i).ToString("yyyy-MM-dd");
            await _cacheService.RemoveAsync(string.Format(FLIGHTS_BY_DATE_KEY, "departure", date));
            await _cacheService.RemoveAsync(string.Format(FLIGHTS_BY_DATE_KEY, "arrival", date));
        }
        
        // For search cache, we'd need pattern removal (Redis-specific)
        await _cacheService.RemoveByPatternAsync("flights:search:*");
        
        _logger.LogDebug("Flight caches invalidated");
    }
}
