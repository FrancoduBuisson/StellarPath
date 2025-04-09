using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class CruiseStatusRepository(IUnitOfWork unitOfWork) : Repository<CruiseStatus>(unitOfWork, "cruise_statuses", "cruise_status_id"), ICruiseStatusRepository
{
    public override async Task<int> AddAsync(CruiseStatus entity)
    {
        var query = @"
            INSERT INTO cruise_statuses (status_name)
            VALUES (@StatusName)
            RETURNING cruise_status_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<CruiseStatus?> GetByNameAsync(string statusName)
    {
        var query = "SELECT * FROM cruise_statuses WHERE status_name = @StatusName";
        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<CruiseStatus>(query, new { StatusName = statusName });
    }

    public override async Task<bool> UpdateAsync(CruiseStatus entity)
    {
        var query = @"
            UPDATE cruise_statuses
            SET status_name = @StatusName
            WHERE cruise_status_id = @CruiseStatusId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }
}