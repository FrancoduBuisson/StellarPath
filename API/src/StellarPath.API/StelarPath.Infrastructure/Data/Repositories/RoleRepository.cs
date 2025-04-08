using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class RoleRepository(IUnitOfWork unitOfWork) : Repository<Role>(unitOfWork, "roles", "role_id"), IRoleRepository
{
    public override async Task<int> AddAsync(Role entity)
    {
        var query = @"
            INSERT INTO roles (role_name)
            VALUES (@RoleName)
            RETURNING role_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<Role> GetRoleByNameAsync(string roleName)
    {
        var query = "SELECT * FROM roles WHERE role_name = @RoleName";
        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<Role>(query, new { RoleName = roleName });
    }

    public override async Task<bool> UpdateAsync(Role entity)
    {
        var query = @"
            UPDATE roles
            SET role_name = @RoleName
            WHERE role_id = @RoleId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }
}