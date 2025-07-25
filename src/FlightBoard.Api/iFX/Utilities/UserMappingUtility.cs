using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;
using FlightBoard.Api.iFX.Contract;

namespace FlightBoard.Api.iFX.Utilities;

/// <summary>
/// User mapping utility implementation
/// Following iDesign Method: Infrastructure framework utilities
/// </summary>
public class UserMappingUtility : IUserMappingUtility
{
    /// <summary>
    /// Maps User entity to UserDto
    /// </summary>
    public UserDto ToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    /// <summary>
    /// Maps User entity collection to UserDto collection
    /// </summary>
    public List<UserDto> ToUserDtoList(List<User> users)
    {
        return users.Select(ToUserDto).ToList();
    }
}
