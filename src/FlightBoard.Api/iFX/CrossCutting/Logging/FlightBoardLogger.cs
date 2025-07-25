namespace FlightBoard.Api.iFX.CrossCutting.Logging;

/// <summary>
/// Enhanced logging helper with structured logging capabilities
/// </summary>
public static class FlightBoardLogger
{
    /// <summary>
    /// Log flight operation with structured data
    /// </summary>
    public static void LogFlightOperation(this ILogger logger, string operation, string flightNumber, object? additionalData = null)
    {
        using (logger.BeginScope("FlightOperation"))
        {
            logger.LogInformation("Flight operation: {Operation} for {FlightNumber}. Data: {@AdditionalData}",
                operation, flightNumber, additionalData);
        }
    }

    /// <summary>
    /// Log API request with performance timing
    /// </summary>
    public static void LogApiRequest(this ILogger logger, string endpoint, string method, long durationMs, object? requestData = null)
    {
        using (logger.BeginScope("ApiRequest"))
        {
            logger.LogInformation("API {Method} {Endpoint} completed in {Duration}ms. Request: {@RequestData}",
                method, endpoint, durationMs, requestData);
        }
    }

    /// <summary>
    /// Log business rule validation
    /// </summary>
    public static void LogValidation(this ILogger logger, string validationType, bool isValid, string[] errors)
    {
        if (isValid)
        {
            logger.LogDebug("Validation passed: {ValidationType}", validationType);
        }
        else
        {
            logger.LogWarning("Validation failed: {ValidationType}. Errors: {Errors}",
                validationType, string.Join(", ", errors));
        }
    }

    /// <summary>
    /// Log SignalR notification events
    /// </summary>
    public static void LogNotification(this ILogger logger, string notificationType, string? targetGroup = null, object? data = null)
    {
        using (logger.BeginScope("SignalRNotification"))
        {
            if (!string.IsNullOrEmpty(targetGroup))
            {
                logger.LogInformation("Broadcasting {NotificationType} to group {TargetGroup}. Data: {@Data}",
                    notificationType, targetGroup, data);
            }
            else
            {
                logger.LogInformation("Broadcasting {NotificationType} to all clients. Data: {@Data}",
                    notificationType, data);
            }
        }
    }

    /// <summary>
    /// Log database operations with performance metrics
    /// </summary>
    public static void LogDatabaseOperation(this ILogger logger, string operation, string entityType, int recordCount, long durationMs)
    {
        using (logger.BeginScope("DatabaseOperation"))
        {
            logger.LogInformation("Database {Operation} on {EntityType}: {RecordCount} records in {Duration}ms",
                operation, entityType, recordCount, durationMs);
        }
    }

    /// <summary>
    /// Log error with contextual information
    /// </summary>
    public static void LogError(this ILogger logger, Exception exception, string operation, object? context = null)
    {
        logger.LogError(exception, "Error in {Operation}. Context: {@Context}", operation, context);
    }
}
