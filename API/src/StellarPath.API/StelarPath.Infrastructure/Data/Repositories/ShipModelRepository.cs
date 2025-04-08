using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class ShipModelRepository(IUnitOfWork unitOfWork) : Repository<ShipModel>(unitOfWork, "ship_models", "model_id"), IShipModelRepository
{
    public override async Task<int> AddAsync(ShipModel entity)
    {
        var query = @"
            INSERT INTO ship_models (model_name, capacity, cruise_speed_kmph)
            VALUES (@ModelName, @Capacity, @CruiseSpeedKmph)
            RETURNING model_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public override async Task<bool> UpdateAsync(ShipModel entity)
    {
        var query = @"
            UPDATE ship_models
            SET model_name = @ModelName,
                capacity = @Capacity,
                cruise_speed_kmph = @CruiseSpeedKmph
            WHERE model_id = @ModelId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<IEnumerable<ShipModel>> SearchShipModelsAsync(
        string? name, int? minCapacity, int? maxCapacity, int? minSpeed, int? maxSpeed)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(name))
        {
            conditions.Add("model_name ILIKE @Name");
            parameters.Add("Name", $"%{name}%");
        }

        if (minCapacity.HasValue)
        {
            conditions.Add("capacity >= @MinCapacity");
            parameters.Add("MinCapacity", minCapacity.Value);
        }

        if (maxCapacity.HasValue)
        {
            conditions.Add("capacity <= @MaxCapacity");
            parameters.Add("MaxCapacity", maxCapacity.Value);
        }

        if (minSpeed.HasValue)
        {
            conditions.Add("cruise_speed_kmph >= @MinSpeed");
            parameters.Add("MinSpeed", minSpeed.Value);
        }

        if (maxSpeed.HasValue)
        {
            conditions.Add("cruise_speed_kmph <= @MaxSpeed");
            parameters.Add("MaxSpeed", maxSpeed.Value);
        }

        var query = $"SELECT * FROM {TableName}";

        if (conditions.Count > 0)
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }

        return await UnitOfWork.Connection.QueryAsync<ShipModel>(query, parameters);
    }
}