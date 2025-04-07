using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;
public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{

    private const int DefaultUserRoleID = 2;

    public async Task<int> CreateUserAsync(string googleId, string email, string firstName, string lastName)
    {
        try
        {
            unitOfWork.BeginTransaction();

            var user = new User
            {
                GoogleId = googleId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                RoleId = DefaultUserRoleID,
                IsActive = true
            };

            var result = await userRepository.CreateUserAsync(user);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<UserDto> GetUserByGoogleIdAsync(string googleId)
    {
        var user = await userRepository.GetByGoogleIdAsync(googleId);
        if (user == null)
        {
            return null!;
        }

        var role = await userRepository.GetUserRoleNameAsync(googleId);

        return new UserDto
        {
            GoogleId = user.GoogleId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            IsActive = user.IsActive
        };
    }

    public async Task<string> GetUserRoleAsync(string googleId)
    {
        return await userRepository.GetUserRoleNameAsync(googleId);
    }

    public async Task<bool> UserExistsAsync(string googleId)
    {
        var user = await userRepository.GetByGoogleIdAsync(googleId);
        return user != null;
    }
}