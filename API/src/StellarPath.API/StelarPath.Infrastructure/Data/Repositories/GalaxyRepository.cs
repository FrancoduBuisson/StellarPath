using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class GalaxyRepository(IUnitOfWork unitOfWork) : Repository<Galaxy>(unitOfWork, "galaxies", "galaxy_id"), IGalaxyRepository
{
    public override async Task<int> AddAsync(Galaxy entity)
    {
        var query = @"
            INSERT INTO galaxies (galaxy_name, is_active)
            VALUES (@GalaxyName, @IsActive)
            RETURNING galaxy_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<IEnumerable<Galaxy>> GetActiveGalaxiesAsync()
    {
        var query = $"SELECT * FROM {TableName} WHERE is_active = true";
        return await UnitOfWork.Connection.QueryAsync<Galaxy>(query);
    }

    public override async Task<bool> UpdateAsync(Galaxy entity)
    {
        var query = @"
            UPDATE galaxies
            SET galaxy_name = @GalaxyName,
                is_active = @IsActive
            WHERE galaxy_id = @GalaxyId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<IEnumerable<Galaxy>> SearchGalaxiesAsync(string? name, bool? isActive)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(name))
        {
            conditions.Add("galaxy_name ILIKE @Name");
            parameters.Add("Name", $"%{name}%");
        }

        if (isActive.HasValue)
        {
            conditions.Add("is_active = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        var query = $"SELECT * FROM {TableName}";

        if (conditions.Count > 0)
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }

        return await UnitOfWork.Connection.QueryAsync<Galaxy>(query, parameters);
    }
}

