using Dapper;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class BookingHistoryRepository(IUnitOfWork unitOfWork) : Repository<BookingHistory>(unitOfWork, "booking_history", "history_id"), IBookingHistoryRepository
{
    public override async Task<int> AddAsync(BookingHistory entity)
    {
        var query = @"
            INSERT INTO booking_history (booking_id, previous_booking_status_id, new_booking_status_id, changed_at)
            VALUES (@BookingId, @PreviousBookingStatusId, @NewBookingStatusId, @ChangedAt)
            RETURNING history_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<IEnumerable<BookingHistory>> GetHistoryForBookingAsync(int bookingId)
    {
        var query = $"SELECT * FROM {TableName} WHERE booking_id = @BookingId ORDER BY changed_at DESC";
        return await UnitOfWork.Connection.QueryAsync<BookingHistory>(query, new { BookingId = bookingId });
    }

    public override async Task<bool> UpdateAsync(BookingHistory entity)
    {
        return false;
    }
}
