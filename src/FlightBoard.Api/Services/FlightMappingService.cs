using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;

namespace FlightBoard.Api.Services;

/// <summary>
/// Service for mapping between Flight entities and DTOs
/// </summary>
public class FlightMappingService
{    /// <summary>
    /// Map Flight entity to FlightDto using modern C# record syntax
    /// </summary>
    public static FlightDto ToDto(Flight flight)
    {
        return new FlightDto
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline,
            Origin = flight.Origin,
            Destination = flight.Destination,
            ScheduledDeparture = flight.ScheduledDeparture,
            ActualDeparture = flight.ActualDeparture,
            ScheduledArrival = flight.ScheduledArrival,
            ActualArrival = flight.ActualArrival,
            Status = flight.Status.ToString(),
            Gate = flight.Gate,
            Terminal = flight.Terminal,
            AircraftType = flight.AircraftType,
            Remarks = flight.Remarks,
            DelayMinutes = flight.DelayMinutes,
            Type = flight.Type.ToString(),
            CreatedAt = flight.CreatedAt,
            UpdatedAt = flight.UpdatedAt,
            IsDelayed = flight.IsDelayed,
            EstimatedDeparture = flight.EstimatedDeparture,
            EstimatedArrival = flight.EstimatedArrival
        };
    }

    /// <summary>
    /// Map collection of Flight entities to FlightDto collection
    /// </summary>
    public static IEnumerable<FlightDto> ToDto(IEnumerable<Flight> flights)
    {
        return flights.Select(ToDto);
    }

    /// <summary>
    /// Map CreateFlightDto to Flight entity
    /// </summary>
    public static Flight ToEntity(CreateFlightDto dto)
    {
        return new Flight
        {
            FlightNumber = dto.FlightNumber,
            Airline = dto.Airline,
            Origin = dto.Origin,
            Destination = dto.Destination,
            ScheduledDeparture = dto.ScheduledDeparture,
            ScheduledArrival = dto.ScheduledArrival,
            Status = Enum.Parse<FlightStatus>(dto.Status),
            Gate = dto.Gate,
            Terminal = dto.Terminal,
            AircraftType = dto.AircraftType,
            Remarks = dto.Remarks,
            DelayMinutes = dto.DelayMinutes,
            Type = Enum.Parse<FlightType>(dto.Type)
        };
    }

    /// <summary>
    /// Update Flight entity with UpdateFlightDto values
    /// </summary>
    public static void UpdateEntity(Flight flight, UpdateFlightDto dto)
    {
        if (!string.IsNullOrEmpty(dto.FlightNumber))
            flight.FlightNumber = dto.FlightNumber;

        if (!string.IsNullOrEmpty(dto.Airline))
            flight.Airline = dto.Airline;

        if (!string.IsNullOrEmpty(dto.Origin))
            flight.Origin = dto.Origin;

        if (!string.IsNullOrEmpty(dto.Destination))
            flight.Destination = dto.Destination;

        if (dto.ScheduledDeparture.HasValue)
            flight.ScheduledDeparture = dto.ScheduledDeparture.Value;

        if (dto.ActualDeparture.HasValue)
            flight.ActualDeparture = dto.ActualDeparture.Value;

        if (dto.ScheduledArrival.HasValue)
            flight.ScheduledArrival = dto.ScheduledArrival.Value;

        if (dto.ActualArrival.HasValue)
            flight.ActualArrival = dto.ActualArrival.Value;

        if (!string.IsNullOrEmpty(dto.Status))
            flight.Status = Enum.Parse<FlightStatus>(dto.Status);

        if (dto.Gate != null)
            flight.Gate = dto.Gate;

        if (dto.Terminal != null)
            flight.Terminal = dto.Terminal;

        if (dto.AircraftType != null)
            flight.AircraftType = dto.AircraftType;

        if (dto.Remarks != null)
            flight.Remarks = dto.Remarks;

        if (dto.DelayMinutes.HasValue)
            flight.DelayMinutes = dto.DelayMinutes.Value;

        if (!string.IsNullOrEmpty(dto.Type))
            flight.Type = Enum.Parse<FlightType>(dto.Type);
    }    /// <summary>
    /// Create a paginated response with modern record syntax
    /// </summary>
    public static PagedResponse<FlightDto> ToPagedResponse(IEnumerable<Flight> flights, int page, int pageSize, int totalCount)
    {
        return new PagedResponse<FlightDto>
        {
            Data = ToDto(flights),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
