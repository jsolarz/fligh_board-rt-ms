using FlightBoard.Api.Core.Entities;
using FlightBoard.Api.Core.DTOs;

namespace FlightBoard.Api.iFX.Utilities.Mapping;

/// <summary>
/// Interface for flight mapping operations following iDesign Method principles
/// </summary>
public interface IFlightMappingUtility
{
    FlightDto MapToDto(Flight flight, FlightStatus calculatedStatus);
    FlightDto MapToDto(Flight flight);
}
