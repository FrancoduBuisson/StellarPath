using Dapper;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;
public class BookingStatusRepository(IUnitOfWork unitOfWork) : Repository<BookingStatus>(unitOfWork, "booking_statuses", "booking_status_id"), IBookingStatusRepository
{
    public override async Task<int> AddAsync(BookingStatus entity)
    {
        var query = @"
            INSERT INTO booking_statuses (status_name)
            VALUES (@StatusName)
            RETURNING booking_status_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<BookingStatus?> GetByNameAsync(string statusName)
    {
        var query = "SELECT * FROM booking_statuses WHERE status_name = @StatusName";
        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<BookingStatus>(query, new { StatusName = statusName });
    }

    public override async Task<bool> UpdateAsync(BookingStatus entity)
    {
        var query = @"
            UPDATE booking_statuses
            SET status_name = @StatusName
            WHERE booking_status_id = @BookingStatusId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }
}