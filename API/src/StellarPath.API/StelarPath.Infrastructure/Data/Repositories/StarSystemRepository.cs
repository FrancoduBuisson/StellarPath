using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class StarSystemRepository(IUnitOfWork unitOfWork) : Repository<StarSystem>(unitOfWork, "star_systems", "system_id"), IStarSystemRepository
{
    public override async Task<int> AddAsync(StarSystem entity)
    {
        var query = @"
            INSERT INTO star_systems (system_name, galaxy_id, is_active)
            VALUES (@SystemName, @GalaxyId, @IsActive)
            RETURNING system_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<IEnumerable<StarSystem>> GetActiveStarSystemsAsync()
    {
        var query = $"SELECT * FROM {TableName} WHERE is_active = true";
        return await UnitOfWork.Connection.QueryAsync<StarSystem>(query);
    }

    public async Task<IEnumerable<StarSystem>> GetStarSystemsByGalaxyIdAsync(int galaxyId)
    {
        var query = $"SELECT * FROM {TableName} WHERE galaxy_id = @GalaxyId";
        return await UnitOfWork.Connection.QueryAsync<StarSystem>(query, new { GalaxyId = galaxyId });
    }

    public override async Task<bool> UpdateAsync(StarSystem entity)
    {
        var query = @"
            UPDATE star_systems
            SET system_name = @SystemName,
                galaxy_id = @GalaxyId,
                is_active = @IsActive
            WHERE system_id = @SystemId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<IEnumerable<StarSystem>> SearchStarSystemsAsync(
    string? name, int? galaxyId, bool? isActive)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(name))
        {
            conditions.Add("system_name ILIKE @Name");
            parameters.Add("Name", $"%{name}%");
        }

        if (galaxyId.HasValue)
        {
            conditions.Add("galaxy_id = @GalaxyId");
            parameters.Add("GalaxyId", galaxyId.Value);
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

        return await UnitOfWork.Connection.QueryAsync<StarSystem>(query, parameters);
    }
}