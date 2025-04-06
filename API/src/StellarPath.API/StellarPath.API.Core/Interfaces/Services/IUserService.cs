using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
internal interface IUserService
{
    Task<bool> UserExistsAsync(string googleId);
    Task<int> CreateUserAsync(string googleId, string email, string firstName, string lastName);
    Task<UserDto> GetUserByGoogleIdAsync(string googleId);
    Task<string> GetUserRoleAsync(string googleId);

}

