using System.ComponentModel.DataAnnotations;

namespace FlightBoard.Api.Validation;

/// <summary>
/// Validation attribute to ensure a DateTime is in the future
/// Required by objectives.md business rules
/// </summary>
public class FutureDateAttribute : ValidationAttribute
{
    /// <summary>
    /// Validates that the provided DateTime value is in the future
    /// </summary>
    /// <param name="value">The DateTime value to validate</param>
    /// <returns>True if the date is in the future, false otherwise</returns>
    public override bool IsValid(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime > DateTime.UtcNow;
        }

        // Allow null values (handled by Required attribute separately)
        return value == null;
    }

    /// <summary>
    /// Gets the default error message
    /// </summary>
    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be a future date and time.";
    }
}
