using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;

public interface IUserProvider
{
    Task<UserDto> GetCurrentUserAsync();
    string GetCurrentUserId();
}