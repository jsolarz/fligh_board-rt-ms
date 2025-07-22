using FlightBoard.Api.DTOs;
using FlightBoard.Api.Models;

namespace FlightBoard.Api.iFX.Contract;

/// <summary>
/// User mapping utility contract
/// Following iDesign Method: Infrastructure framework utilities
/// </summary>
public interface IUserMappingUtility
{
    /// <summary>
    /// Maps User entity to UserDto
    /// </summary>
    UserDto ToUserDto(User user);

    /// <summary>
    /// Maps User entity collection to UserDto collection
    /// </summary>
    List<UserDto> ToUserDtoList(List<User> users);
}
