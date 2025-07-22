using FlightBoard.Api.Models;
using FlightBoard.Api.DTOs;

namespace FlightBoard.Api.Engines;

/// <summary>
/// Interface for flight business logic operations following iDesign Method principles
/// </summary>
public interface IFlightEngine
{
    FlightStatus CalculateFlightStatus(DateTime scheduledDeparture, DateTime? actualDeparture = null);
    IQueryable<Flight> ApplySearchFilters(IQueryable<Flight> query, FlightSearchDto searchDto);
    void ValidateFlightForCreation(CreateFlightDto createDto);
    void ValidateFlightForUpdate(UpdateFlightDto updateDto, Flight existingFlight);
    Flight MapCreateDtoToEntity(CreateFlightDto createDto);
    Flight MapUpdateDtoToEntity(UpdateFlightDto updateDto, Flight existingFlight);
}
