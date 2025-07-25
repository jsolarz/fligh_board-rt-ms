using FlightBoard.Api.Core.Entities;
using FlightBoard.Api.Core.DTOs;

namespace FlightBoard.Api.Engines;

/// <summary>
/// Interface for flight business logic operations following iDesign Method principles
/// </summary>
public interface IFlightEngine
{
    FlightStatus CalculateFlightStatus(DateTime scheduledDeparture, DateTime? actualDeparture = null);
    IQueryable<Core.Entities.Flight> ApplySearchFilters(IQueryable<Core.Entities.Flight> query, FlightSearchDto searchDto);
    void ValidateFlightForCreation(CreateFlightDto createDto);
    void ValidateFlightForUpdate(UpdateFlightDto updateDto, Core.Entities.Flight existingFlight);
    Core.Entities.Flight MapCreateDtoToEntity(CreateFlightDto createDto);
    Core.Entities.Flight MapUpdateDtoToEntity(UpdateFlightDto updateDto, Core.Entities.Flight existingFlight);
}
