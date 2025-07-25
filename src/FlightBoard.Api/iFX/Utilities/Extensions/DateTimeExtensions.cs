namespace FlightBoard.Api.iFX.Utilities.Extensions;

/// <summary>
/// Extension methods for DateTime operations in the flight domain
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Calculate minutes until departure from now
    /// </summary>
    public static double MinutesUntilDeparture(this DateTime scheduledDeparture)
    {
        return (scheduledDeparture - DateTime.UtcNow).TotalMinutes;
    }

    /// <summary>
    /// Calculate minutes since departure from now
    /// </summary>
    public static double MinutesSinceDeparture(this DateTime? actualDeparture)
    {
        if (!actualDeparture.HasValue) return 0;
        return (DateTime.UtcNow - actualDeparture.Value).TotalMinutes;
    }

    /// <summary>
    /// Check if flight is departing soon (within specified minutes)
    /// </summary>
    public static bool IsDepartingSoon(this DateTime scheduledDeparture, int withinMinutes = 30)
    {
        var minutesUntil = scheduledDeparture.MinutesUntilDeparture();
        return minutesUntil > 0 && minutesUntil <= withinMinutes;
    }

    /// <summary>
    /// Check if flight has recently departed (within specified minutes)
    /// </summary>
    public static bool HasRecentlyDeparted(this DateTime? actualDeparture, int withinMinutes = 60)
    {
        if (!actualDeparture.HasValue) return false;
        var minutesSince = actualDeparture.MinutesSinceDeparture();
        return minutesSince >= 0 && minutesSince <= withinMinutes;
    }
}
