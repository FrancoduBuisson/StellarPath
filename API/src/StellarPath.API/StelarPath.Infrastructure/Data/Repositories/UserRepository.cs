using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;
using static Dapper.SqlMapper;

namespace StelarPath.API.Infrastructure.Data.Repositories;
public class UserRepository(IUnitOfWork unitOfWork) : Repository<User>(unitOfWork, "users", "google_id"), IUserRepository
{
    public override async Task<int> AddAsync(User entity)
    {
        var query = @"
            INSERT INTO users (google_id, email, first_name, last_name, role_id, is_active)
            VALUES (@GoogleId, @Email, @FirstName, @LastName, @RoleId, @IsActive)"
        ;

        await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return 1;
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        var query = "SELECT * FROM users WHERE google_id = @GoogleId";
        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<User>(query, new { GoogleId = googleId });
    }

    public async Task<string> GetUserRoleNameAsync(string googleId)
    {
        var query = @"
            SELECT r.role_name FROM users u
            JOIN roles r ON u.role_id = r.role_id
            WHERE u.google_id = @GoogleId";

        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<string>(query, new { GoogleId = googleId });
    }

    public override async Task<bool> UpdateAsync(User entity)
    {
        var query = @"
            UPDATE users
            SET email = @Email,
                first_name = @FirstName,
                last_name = @LastName,
                role_id = @RoleId,
                is_active = @IsActive
            WHERE google_id = @GoogleId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        return await AddAsync(user);
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException("Users cannot be deleted by numeric ID");
    }

    public async Task<bool> DeleteUserGoogleID(string googleId)
    {
        var query = $"DELETE FROM {TableName} WHERE google_id = @GoogleId";
        return await UnitOfWork.Connection.ExecuteAsync(query, new { GoogleId = googleId}) > 0;
    }
}

