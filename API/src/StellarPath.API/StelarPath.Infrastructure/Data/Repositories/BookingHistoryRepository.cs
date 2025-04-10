using Dapper;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StelarPath.API.Infrastructure.Data.Repositories
{
    public class BookingHistoryRepository(IUnitOfWork unitOfWork)
        : Repository<BookingHistory>(unitOfWork, "booking_history", "history_id"), IBookingHistoryRepository
    {
        public override async Task<int> AddAsync(BookingHistory entity)
        {
            var query = @"
                INSERT INTO booking_history (
                    booking_id, 
                    previous_booking_status_id, 
                    new_booking_status_id, 
                    changed_at
                )
                VALUES (
                    @BookingId, 
                    @PreviousBookingStatusId, 
                    @NewBookingStatusId, 
                    @ChangedAt
                );
                SELECT LASTVAL();";

            return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
        }

        public override async Task<bool> UpdateAsync(BookingHistory entity)
        {
            var query = @"
                UPDATE booking_history
                SET 
                    booking_id = @BookingId,
                    previous_booking_status_id = @PreviousBookingStatusId,
                    new_booking_status_id = @NewBookingStatusId,
                    changed_at = @ChangedAt
                WHERE history_id = @HistoryId";

            var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
            return result > 0;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var query = $"DELETE FROM {TableName} WHERE history_id = @Id";
            return await UnitOfWork.Connection.ExecuteAsync(query, new { Id = id }) > 0;
        }

        public async Task<IEnumerable<BookingHistory>> GetHistoryForBookingAsync(int bookingId)
        {
            var query = @"
                SELECT * FROM booking_history 
                WHERE booking_id = @BookingId
                ORDER BY changed_at DESC";

            return await UnitOfWork.Connection.QueryAsync<BookingHistory>(query,
                new { BookingId = bookingId });
        }

        public async Task<BookingHistory> AddHistoryRecordAsync(BookingHistory history)
        {
            var query = @"
                INSERT INTO booking_history (
                    booking_id, 
                    previous_booking_status_id, 
                    new_booking_status_id, 
                    changed_at
                )
                VALUES (
                    @BookingId, 
                    @PreviousBookingStatusId, 
                    @NewBookingStatusId, 
                    @ChangedAt
                )
                RETURNING *;";

            return await UnitOfWork.Connection.QuerySingleAsync<BookingHistory>(query, history);
        }

        public async Task<bool> UpdateHistoryRecordAsync(BookingHistory history)
        {
            return await UpdateAsync(history);
        }

        public async Task<int> AddStatusChangeAsync(int bookingId, int oldStatusId, int newStatusId)
        {
            var historyRecord = new BookingHistory
            {
                BookingId = bookingId,
                PreviousBookingStatusId = oldStatusId,
                NewBookingStatusId = newStatusId,
                ChangedAt = DateTime.UtcNow
            };

            return await AddAsync(historyRecord);
        }

        public async Task<IEnumerable<BookingHistory>> GetRecentStatusChangesAsync(int limit = 100)
        {
            var query = @"
                SELECT * FROM booking_history
                ORDER BY changed_at DESC
                LIMIT @Limit";

            return await UnitOfWork.Connection.QueryAsync<BookingHistory>(query,
                new { Limit = limit });
        }
    }
}