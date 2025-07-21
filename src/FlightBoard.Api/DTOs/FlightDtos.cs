using System.ComponentModel.DataAnnotations;
using FlightBoard.Api.Validation;

namespace FlightBoard.Api.DTOs;

/// <summary>
/// DTO for flight responses to clients using modern C# record syntax
/// </summary>
public sealed record FlightDto
{
    public required int Id { get; init; }
    public required string FlightNumber { get; init; }
    public required string Airline { get; init; }
    public required string Origin { get; init; }
    public required string Destination { get; init; }
    public required DateTime ScheduledDeparture { get; init; }
    public DateTime? ActualDeparture { get; init; }
    public required DateTime ScheduledArrival { get; init; }
    public DateTime? ActualArrival { get; init; }
    public required string Status { get; init; }
    public string? Gate { get; init; }
    public string? Terminal { get; init; }
    public string? AircraftType { get; init; }
    public string? Remarks { get; init; }
    public required int DelayMinutes { get; init; }
    public required string Type { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    // Computed properties
    public required bool IsDelayed { get; init; }
    public required DateTime EstimatedDeparture { get; init; }
    public required DateTime EstimatedArrival { get; init; }
}

/// <summary>
/// DTO for creating new flights with validation attributes
/// </summary>
public sealed record CreateFlightDto
{
    [Required(ErrorMessage = "Flight number is required")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Flight number must be between 3 and 10 characters")]
    public required string FlightNumber { get; init; }

    [Required(ErrorMessage = "Airline code is required")]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "Airline code must be between 2 and 3 characters")]
    public required string Airline { get; init; }

    [Required(ErrorMessage = "Origin airport is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Origin must be 3 characters")]
    public required string Origin { get; init; }

    [Required(ErrorMessage = "Destination airport is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Destination must be 3 characters")]
    public required string Destination { get; init; }    [Required(ErrorMessage = "Scheduled departure is required")]
    [FutureDate(ErrorMessage = "Scheduled departure must be in the future")]
    public required DateTime ScheduledDeparture { get; init; }

    [Required(ErrorMessage = "Scheduled arrival is required")]
    public required DateTime ScheduledArrival { get; init; }

    [Required(ErrorMessage = "Flight status is required")]
    public string Status { get; init; } = "Scheduled";

    [StringLength(10, ErrorMessage = "Gate cannot exceed 10 characters")]
    public string? Gate { get; init; }

    [StringLength(20, ErrorMessage = "Terminal cannot exceed 20 characters")]
    public string? Terminal { get; init; }

    [StringLength(50, ErrorMessage = "Aircraft type cannot exceed 50 characters")]
    public string? AircraftType { get; init; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; init; }

    [Range(-120, 720, ErrorMessage = "Delay minutes must be between -120 and 720")]
    public int DelayMinutes { get; init; } = 0;

    [Required(ErrorMessage = "Flight type is required")]
    public string Type { get; init; } = "Departure";

    // Custom validation method
    public bool IsValid(out string[] errors)
    {
        var errorList = new List<string>();

        if (ScheduledArrival <= ScheduledDeparture)
            errorList.Add("Scheduled arrival must be after scheduled departure");

        if (Origin.Equals(Destination, StringComparison.OrdinalIgnoreCase))
            errorList.Add("Origin and destination cannot be the same");

        errors = errorList.ToArray();
        return errorList.Count == 0;
    }
}

/// <summary>
/// DTO for updating existing flights with optional properties
/// </summary>
public sealed record UpdateFlightDto
{
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Flight number must be between 3 and 10 characters")]
    public string? FlightNumber { get; init; }

    [StringLength(3, MinimumLength = 2, ErrorMessage = "Airline code must be between 2 and 3 characters")]
    public string? Airline { get; init; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Origin must be 3 characters")]
    public string? Origin { get; init; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Destination must be 3 characters")]
    public string? Destination { get; init; }

    public DateTime? ScheduledDeparture { get; init; }
    public DateTime? ActualDeparture { get; init; }
    public DateTime? ScheduledArrival { get; init; }
    public DateTime? ActualArrival { get; init; }
    public string? Status { get; init; }

    [StringLength(10, ErrorMessage = "Gate cannot exceed 10 characters")]
    public string? Gate { get; init; }

    [StringLength(20, ErrorMessage = "Terminal cannot exceed 20 characters")]
    public string? Terminal { get; init; }

    [StringLength(50, ErrorMessage = "Aircraft type cannot exceed 50 characters")]
    public string? AircraftType { get; init; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; init; }

    [Range(-120, 720, ErrorMessage = "Delay minutes must be between -120 and 720")]
    public int? DelayMinutes { get; init; }

    public string? Type { get; init; }
}

/// <summary>
/// DTO for flight search/filter parameters with default values
/// </summary>
public sealed record FlightSearchDto
{
    public string? FlightNumber { get; init; }
    public string? Airline { get; init; }
    public string? Origin { get; init; }
    public string? Destination { get; init; }
    public string? Status { get; init; }
    public string? Type { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool? IsDelayed { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Paginated response wrapper using modern record syntax
/// </summary>
/// <typeparam name="T">Type of data being paginated</typeparam>
public sealed record PagedResponse<T>
{
    public required IEnumerable<T> Data { get; init; } = Enumerable.Empty<T>();
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
    public bool HasData => Data.Any();
    public int CurrentPageSize => Data.Count();
}

/// <summary>
/// Request model for updating flight status using primary constructor
/// </summary>
/// <param name="Status">New flight status</param>
/// <param name="Remarks">Optional remarks</param>
public sealed record UpdateStatusRequest(
    [Required(ErrorMessage = "Status is required")] string Status,
    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")] string? Remarks = null
);

/// <summary>
/// API response wrapper for consistent response format
/// </summary>
/// <typeparam name="T">Type of response data</typeparam>
public sealed record ApiResponse<T>
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string[]? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> ErrorResponse(string message, string[]? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
