using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<string> GetUserRoleNameAsync(string googleId);
    Task<int> CreateUserAsync(User user);
    Task<bool> DeleteUserGoogleID(string googleId);
}

