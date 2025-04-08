using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class DestinationRepository(IUnitOfWork unitOfWork) : Repository<Destination>(unitOfWork, "destinations", "destination_id"), IDestinationRepository
{
    public override async Task<int> AddAsync(Destination entity)
    {
        var query = @"
            INSERT INTO destinations (name, system_id, distance_from_earth, is_active)
            VALUES (@Name, @SystemId, @DistanceFromEarth, @IsActive)
            RETURNING destination_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<IEnumerable<Destination>> GetActiveDestinationsAsync()
    {
        var query = $"SELECT * FROM {TableName} WHERE is_active = true";
        return await UnitOfWork.Connection.QueryAsync<Destination>(query);
    }

    public async Task<IEnumerable<Destination>> GetDestinationsBySystemIdAsync(int systemId)
    {
        var query = $"SELECT * FROM {TableName} WHERE system_id = @SystemId";
        return await UnitOfWork.Connection.QueryAsync<Destination>(query, new { SystemId = systemId });
    }

    public override async Task<bool> UpdateAsync(Destination entity)
    {
        var query = @"
            UPDATE destinations
            SET name = @Name,
                system_id = @SystemId,
                distance_from_earth = @DistanceFromEarth,
                is_active = @IsActive
            WHERE destination_id = @DestinationId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<IEnumerable<Destination>> SearchDestinationsAsync(
    string? name, int? systemId, long? minDistance,
    long? maxDistance, bool? isActive)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(name))
        {
            conditions.Add("name ILIKE @Name");
            parameters.Add("Name", $"%{name}%");
        }

        if (systemId.HasValue)
        {
            conditions.Add("system_id = @SystemId");
            parameters.Add("SystemId", systemId.Value);
        }

        if (minDistance.HasValue)
        {
            conditions.Add("distance_from_earth >= @MinDistance");
            parameters.Add("MinDistance", minDistance.Value);
        }

        if (maxDistance.HasValue)
        {
            conditions.Add("distance_from_earth <= @MaxDistance");
            parameters.Add("MaxDistance", maxDistance.Value);
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

        return await UnitOfWork.Connection.QueryAsync<Destination>(query, parameters);
    }
}