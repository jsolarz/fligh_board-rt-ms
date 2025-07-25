using FlightBoard.Api.iFX.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace FlightBoard.Api.iFX.Service;

/// <summary>
/// Infrastructure framework service implementation
/// Following iDesign Method: iFX.Service contains infrastructure service implementations
/// </summary>
public class iFXServiceManager : FlightBoard.Api.iFX.Contract.IServiceCollectionExtensions
{
    public Microsoft.Extensions.DependencyInjection.IServiceCollection RegisteriFXServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        // Use the existing iFX extension method
        services.AddiFXServices();

        return services;
    }
}
