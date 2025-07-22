namespace FlightBoard.Api.iFX.Contract;

/// <summary>
/// Contract for infrastructure framework services
/// Following iDesign Method: iFX contracts define infrastructure service interfaces
/// </summary>
public interface IServiceCollectionExtensions
{
    /// <summary>
    /// Register infrastructure framework services
    /// </summary>
    IServiceCollection RegisteriFXServices(IServiceCollection services);
}

/// <summary>
/// Contract for cross-cutting notification services
/// </summary>
public interface INotificationEngine
{
    Task NotifyFlightCreatedAsync(string flightNumber, object flightData);
    Task NotifyFlightUpdatedAsync(string flightNumber, object flightData, object previousData);
    Task NotifyFlightStatusChangedAsync(string flightNumber, string oldStatus, string newStatus, object flightData);
}

/// <summary>
/// Contract for utility mapping services
/// </summary>
public interface IMappingUtility<TEntity, TDto>
{
    TDto MapToDto(TEntity entity);
    TEntity MapToEntity(TDto dto);
}
