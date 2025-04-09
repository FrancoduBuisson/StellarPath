using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class SpaceshipRepository(IUnitOfWork unitOfWork) : Repository<Spaceship>(unitOfWork, "spaceships", "spaceship_id"), ISpaceshipRepository
{
    public override async Task<int> AddAsync(Spaceship entity)
    {
        var query = @"
            INSERT INTO spaceships (model_id, is_active)
            VALUES (@ModelId, @IsActive)
            RETURNING spaceship_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<IEnumerable<Spaceship>> GetActiveSpaceshipsAsync()
    {
        var query = $"SELECT * FROM {TableName} WHERE is_active = true";
        return await UnitOfWork.Connection.QueryAsync<Spaceship>(query);
    }

    public async Task<IEnumerable<Spaceship>> GetSpaceshipsByModelIdAsync(int modelId)
    {
        var query = $"SELECT * FROM {TableName} WHERE model_id = @ModelId";
        return await UnitOfWork.Connection.QueryAsync<Spaceship>(query, new { ModelId = modelId });
    }

    public override async Task<bool> UpdateAsync(Spaceship entity)
    {
        var query = @"
            UPDATE spaceships
            SET model_id = @ModelId,
                is_active = @IsActive
            WHERE spaceship_id = @SpaceshipId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<IEnumerable<Spaceship>> SearchSpaceshipsAsync(
        int? modelId, string? modelName, bool? isActive)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (modelId.HasValue)
        {
            conditions.Add("s.model_id = @ModelId");
            parameters.Add("ModelId", modelId.Value);
        }

        if (!string.IsNullOrEmpty(modelName))
        {
            conditions.Add("m.model_name ILIKE @ModelName");
            parameters.Add("ModelName", $"%{modelName}%");
        }

        if (isActive.HasValue)
        {
            conditions.Add("s.is_active = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        var query = $@"
            SELECT s.*
            FROM {TableName} s
            LEFT JOIN ship_models m ON s.model_id = m.model_id";

        if (conditions.Count > 0)
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }

        return await UnitOfWork.Connection.QueryAsync<Spaceship>(query, parameters);
    }
}