namespace FlightBoard.Api.iFX.Utility;

/// <summary>
/// Cache key utility for generating consistent cache keys
/// Following iDesign Method: Infrastructure utility
/// </summary>
public static class CacheKeys
{
    // Flight Data Keys
    public const string FLIGHTS_ALL = "flights:all";
    public const string FLIGHTS_DEPARTURE = "flights:departure:{0}"; // {0} = date
    public const string FLIGHTS_ARRIVAL = "flights:arrival:{0}"; // {0} = date
    public const string FLIGHTS_STATUS = "flights:status:{0}"; // {0} = status
    public const string FLIGHTS_AIRLINE = "flights:airline:{0}"; // {0} = airline
    public const string FLIGHT_DETAIL = "flight:detail:{0}"; // {0} = id

    // Airport Data Keys  
    public const string AIRPORTS_ALL = "airports:all";
    public const string AIRPORT_DETAIL = "airport:detail:{0}"; // {0} = code

    // Airlines Data Keys
    public const string AIRLINES_ALL = "airlines:all";
    public const string AIRLINE_DETAIL = "airline:detail:{0}"; // {0} = code

    // User/Auth Keys
    public const string USER_DETAIL = "user:detail:{0}"; // {0} = userId
    public const string USER_PERMISSIONS = "user:permissions:{0}"; // {0} = userId

    // System Data Keys
    public const string SYSTEM_CONFIG = "system:config";
    public const string SYSTEM_STATUS = "system:status";

    /// <summary>
    /// Formats a cache key with parameters
    /// </summary>
    public static string Format(string template, params object[] args)
    {
        return string.Format(template, args);
    }

    /// <summary>
    /// Generates pattern for cache invalidation
    /// </summary>
    public static string GetPattern(string prefix)
    {
        return $"{prefix}:*";
    }

    /// <summary>
    /// Gets base prefix from a formatted key
    /// </summary>
    public static string GetPrefix(string key)
    {
        var colonIndex = key.IndexOf(':');
        return colonIndex > 0 ? key.Substring(0, colonIndex) : key;
    }
}
