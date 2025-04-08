using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IUserService
{
    Task<bool> UserExistsAsync(string googleId);
    Task<int> CreateUserAsync(string googleId, string email, string firstName, string lastName);
    Task<UserDto> GetUserByGoogleIdAsync(string googleId);
    Task<string> GetUserRoleAsync(string googleId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> DeactivateUserAsync(string googleId);
    Task<bool> ActivateUserAsync(string googleId);
    Task<bool> UpdateUserRoleAsync(string googleId, string roleName);
    Task<IEnumerable<UserDto>> SearchUsersAsync(
    string? name, string? firstName, string? lastName,
    string? email, string? role, bool? isActive);

}

