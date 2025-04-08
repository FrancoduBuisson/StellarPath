using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;
public class UserService(IUserRepository userRepository, IRoleRepository roleRepository, IUnitOfWork unitOfWork) : IUserService
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

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var role = await userRepository.GetUserRoleNameAsync(user.GoogleId);
            userDtos.Add(new UserDto
            {
                GoogleId = user.GoogleId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = role,
                IsActive = user.IsActive
            });
        }

        return userDtos;
    }

    public async Task<bool> DeactivateUserAsync(string googleId)
    {
        try
        {
            var user = await userRepository.GetByGoogleIdAsync(googleId);
            if (user == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            user.IsActive = false;
            var result = await userRepository.UpdateAsync(user);

            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> ActivateUserAsync(string googleId)
    {
        try
        {
            var user = await userRepository.GetByGoogleIdAsync(googleId);
            if (user == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            user.IsActive = true;
            var result = await userRepository.UpdateAsync(user);

            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateUserRoleAsync(string googleId, string roleName)
    {
        try
        {
            var role = await roleRepository.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                return false;
            }

            var user = await userRepository.GetByGoogleIdAsync(googleId);
            if (user == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            user.RoleId = role.RoleId;
            var result = await userRepository.UpdateAsync(user);

            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> SearchUsersAsync(
    string? name, string? firstName, string? lastName,
    string? email, string? role, bool? isActive)
    {
        int? roleId = null;

        if (!string.IsNullOrEmpty(role))
        {
            var roleEntity = await roleRepository.GetRoleByNameAsync(role);
            roleId = roleEntity?.RoleId;
        }

        var users = await userRepository.SearchUsersAsync(
            name, firstName, lastName, email, roleId, isActive);

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roleName = await userRepository.GetUserRoleNameAsync(user.GoogleId);
            userDtos.Add(new UserDto
            {
                GoogleId = user.GoogleId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roleName,
                IsActive = user.IsActive
            });
        }

        return userDtos;
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