using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface IRoleRepository : IRepository<Role>
{
    Task<Role> GetRoleByNameAsync(string roleName);
}