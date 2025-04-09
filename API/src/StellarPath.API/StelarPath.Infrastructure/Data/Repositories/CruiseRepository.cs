using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class CruiseRepository(IUnitOfWork unitOfWork) : Repository<Cruise>(unitOfWork, "cruises", "cruise_id"), ICruiseRepository
{
    public override async Task<int> AddAsync(Cruise entity)
    {
        var query = @"
            INSERT INTO cruises (
                spaceship_id, 
                departure_destination_id, 
                arrival_destination_id, 
                local_departure_time, 
                duration_minutes, 
                cruise_seat_price, 
                cruise_status_id, 
                created_by_google_id)
            VALUES (
                @SpaceshipId, 
                @DepartureDestinationId, 
                @ArrivalDestinationId, 
                @LocalDepartureTime, 
                @DurationMinutes, 
                @CruiseSeatPrice, 
                @CruiseStatusId, 
                @CreatedByGoogleId)
            RETURNING cruise_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public override async Task<bool> UpdateAsync(Cruise entity)
    {
        var query = @"
            UPDATE cruises
            SET spaceship_id = @SpaceshipId,
                departure_destination_id = @DepartureDestinationId,
                arrival_destination_id = @ArrivalDestinationId,
                local_departure_time = @LocalDepartureTime,
                duration_minutes = @DurationMinutes,
                cruise_seat_price = @CruiseSeatPrice,
                cruise_status_id = @CruiseStatusId
            WHERE cruise_id = @CruiseId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<bool> UpdateCruiseStatusAsync(int cruiseId, int statusId)
    {
        var query = @"
            UPDATE cruises
            SET cruise_status_id = @StatusId
            WHERE cruise_id = @CruiseId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, new { CruiseId = cruiseId, StatusId = statusId });
        return result > 0;
    }

    public async Task<IEnumerable<Cruise>> GetCruisesBySpaceshipIdAsync(int spaceshipId)
    {
        var query = $"SELECT * FROM {TableName} WHERE spaceship_id = @SpaceshipId";
        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, new { SpaceshipId = spaceshipId });
    }

    public async Task<IEnumerable<Cruise>> GetCruisesByStatusAsync(int statusId)
    {
        var query = $"SELECT * FROM {TableName} WHERE cruise_status_id = @StatusId";
        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, new { StatusId = statusId });
    }

    public async Task<IEnumerable<Cruise>> GetCruisesByDepartureDestinationAsync(int destinationId)
    {
        var query = $"SELECT * FROM {TableName} WHERE departure_destination_id = @DestinationId";
        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, new { DestinationId = destinationId });
    }

    public async Task<IEnumerable<Cruise>> GetCruisesByArrivalDestinationAsync(int destinationId)
    {
        var query = $"SELECT * FROM {TableName} WHERE arrival_destination_id = @DestinationId";
        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, new { DestinationId = destinationId });
    }

    public async Task<IEnumerable<Cruise>> GetCruisesBetweenDatesAsync(DateTime startDate, DateTime endDate)
    {
        var query = $"SELECT * FROM {TableName} WHERE local_departure_time BETWEEN @StartDate AND @EndDate";
        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<IEnumerable<Cruise>> GetCruisesCreatedByUserAsync(string googleId)
    {
        var query = $"SELECT * FROM {TableName} WHERE created_by_google_id = @GoogleId";
        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, new { GoogleId = googleId });
    }

    public async Task<IEnumerable<Cruise>> SearchCruisesAsync(
        int? spaceshipId,
        int? departureDestinationId,
        int? arrivalDestinationId,
        DateTime? startDate,
        DateTime? endDate,
        int? statusId,
        decimal? minPrice,
        decimal? maxPrice)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (spaceshipId.HasValue)
        {
            conditions.Add("spaceship_id = @SpaceshipId");
            parameters.Add("SpaceshipId", spaceshipId.Value);
        }

        if (departureDestinationId.HasValue)
        {
            conditions.Add("departure_destination_id = @DepartureDestinationId");
            parameters.Add("DepartureDestinationId", departureDestinationId.Value);
        }

        if (arrivalDestinationId.HasValue)
        {
            conditions.Add("arrival_destination_id = @ArrivalDestinationId");
            parameters.Add("ArrivalDestinationId", arrivalDestinationId.Value);
        }

        if (startDate.HasValue)
        {
            conditions.Add("local_departure_time >= @StartDate");
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            conditions.Add("local_departure_time <= @EndDate");
            parameters.Add("EndDate", endDate.Value);
        }

        if (statusId.HasValue)
        {
            conditions.Add("cruise_status_id = @StatusId");
            parameters.Add("StatusId", statusId.Value);
        }

        if (minPrice.HasValue)
        {
            conditions.Add("cruise_seat_price >= @MinPrice");
            parameters.Add("MinPrice", minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            conditions.Add("cruise_seat_price <= @MaxPrice");
            parameters.Add("MaxPrice", maxPrice.Value);
        }

        var query = $"SELECT * FROM {TableName}";

        if (conditions.Count > 0)
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }

        return await UnitOfWork.Connection.QueryAsync<Cruise>(query, parameters);
    }

    public async Task<IEnumerable<Cruise>> GetOverlappingCruisesForSpaceshipAsync(int spaceshipId, DateTime startTime, DateTime endTime)
    {
        var query = $@"
            SELECT * FROM {TableName} 
            WHERE spaceship_id = @SpaceshipId
            AND cruise_status_id != @CancelledStatusId
            AND (
                (local_departure_time <= @StartTime AND (local_departure_time + (duration_minutes * interval '1 minute')) >= @StartTime)
                OR 
                (local_departure_time <= @EndTime AND (local_departure_time + (duration_minutes * interval '1 minute')) >= @EndTime)
                OR 
                (local_departure_time >= @StartTime AND (local_departure_time + (duration_minutes * interval '1 minute')) <= @EndTime)
            )";

        return await UnitOfWork.Connection.QueryAsync<Cruise>(query,
            new
            {
                SpaceshipId = spaceshipId,
                StartTime = startTime,
                EndTime = endTime,
                CancelledStatusId = 4
            });
    }

    public override async Task<Cruise?> GetByIdAsync(int id)
    {
        var query = $"SELECT * FROM {TableName} WHERE cruise_id = @Id";
        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<Cruise>(query, new { Id = id });
    }
}