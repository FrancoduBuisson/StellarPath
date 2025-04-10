using Dapper;
using StelarPath.API.Infrastructure.Data.Repositories;
using StelarPath.API.Infrastructure.Data;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StelarPath.API.Infrastructure.Data.Repositories
{
    public class BookingRepository(IUnitOfWork unitOfWork)
        : Repository<Booking>(unitOfWork, "bookings", "booking_id"), IBookingRepository
    {
        public override async Task<int> AddAsync(Booking entity)
        {
            var query = @"
                INSERT INTO bookings (
                    google_id, 
                    cruise_id, 
                    seat_number, 
                    booking_date, 
                    booking_expiration, 
                    booking_status_id
                )
                VALUES (
                    @GoogleId, 
                    @CruiseId, 
                    @SeatNumber, 
                    @BookingDate, 
                    @BookingExpiration, 
                    @BookingStatusId
                );
                SELECT LASTVAL();";

            return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
        }

        public override async Task<bool> UpdateAsync(Booking entity)
        {
            var query = @"
                UPDATE bookings
                SET 
                    google_id = @GoogleId,
                    cruise_id = @CruiseId,
                    seat_number = @SeatNumber,
                    booking_date = @BookingDate,
                    booking_expiration = @BookingExpiration,
                    booking_status_id = @BookingStatusId
                WHERE booking_id = @BookingId";

            var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
            return result > 0;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var query = $"DELETE FROM {TableName} WHERE booking_id = @Id";
            return await UnitOfWork.Connection.ExecuteAsync(query, new { Id = id }) > 0;
        }

        // User-specific queries
        public async Task<IEnumerable<Booking>> GetBookingsForUserAsync(string googleId)
        {
            var query = "SELECT * FROM bookings WHERE google_id = @GoogleId";
            return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { GoogleId = googleId });
        }

        public async Task<IEnumerable<Booking>> GetActiveBookingsForUserAsync(string googleId)
        {
            var query = @"
                SELECT b.* FROM bookings b
                JOIN booking_statuses bs ON b.booking_status_id = bs.booking_status_id
                WHERE b.google_id = @GoogleId 
                AND bs.status_name NOT IN ('Cancelled', 'Expired', 'Completed')";

            return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { GoogleId = googleId });
        }

        public async Task<IEnumerable<Booking>> GetExpiredBookingsForUserAsync(string googleId)
        {
            var query = @"
                SELECT b.* FROM bookings b
                JOIN booking_statuses bs ON b.booking_status_id = bs.booking_status_id
                WHERE b.google_id = @GoogleId 
                AND bs.status_name = 'Expired'";

            return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { GoogleId = googleId });
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusForUserAsync(string googleId, int bookingStatusId)
        {
            var query = "SELECT * FROM bookings WHERE google_id = @GoogleId AND booking_status_id = @StatusId";
            return await UnitOfWork.Connection.QueryAsync<Booking>(query,
                new { GoogleId = googleId, StatusId = bookingStatusId });
        }

        // Cruise-specific queries
        public async Task<IEnumerable<Booking>> GetBookingsForCruiseAsync(int cruiseId)
        {
            var query = "SELECT * FROM bookings WHERE cruise_id = @CruiseId";
            return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { CruiseId = cruiseId });
        }

        public async Task<int> GetBookedSeatsCountForCruiseAsync(int cruiseId)
        {
            var query = @"
                SELECT COUNT(*) FROM bookings
                WHERE cruise_id = @CruiseId
                AND booking_status_id NOT IN (
                    SELECT booking_status_id FROM booking_statuses 
                    WHERE status_name IN ('Cancelled', 'Expired')
                )";

            return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, new { CruiseId = cruiseId });
        }

        public async Task<bool> IsSeatAvailableForCruiseAsync(int cruiseId, int seatNumber)
        {
            var query = @"
                SELECT NOT EXISTS (
                    SELECT 1 FROM bookings
                    WHERE cruise_id = @CruiseId
                    AND seat_number = @SeatNumber
                    AND booking_status_id NOT IN (
                        SELECT booking_status_id FROM booking_statuses 
                        WHERE status_name IN ('Cancelled', 'Expired')
                    )
                )";

            return await UnitOfWork.Connection.ExecuteScalarAsync<bool>(query,
                new { CruiseId = cruiseId, SeatNumber = seatNumber });
        }

        // Booking status operations
        public async Task<bool> UpdateBookingStatusAsync(int bookingId, int newStatusId)
        {
            var query = "UPDATE bookings SET booking_status_id = @NewStatusId WHERE booking_id = @BookingId";
            var result = await UnitOfWork.Connection.ExecuteAsync(query,
                new { BookingId = bookingId, NewStatusId = newStatusId });
            return result > 0;
        }

        public async Task<IEnumerable<BookingHistory>> GetBookingHistoryAsync(int bookingId)
        {
            var query = "SELECT * FROM booking_history WHERE booking_id = @BookingId";
            return await UnitOfWork.Connection.QueryAsync<BookingHistory>(query,
                new { BookingId = bookingId });
        }

        // Extras
        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(DateTime fromDate)
        {
            var query = @"
                SELECT b.* FROM bookings b
                JOIN cruises c ON b.cruise_id = c.cruise_id
                WHERE c.local_departure_time >= @FromDate";

            return await UnitOfWork.Connection.QueryAsync<Booking>(query,
                new { FromDate = fromDate });
        }

        public async Task<IEnumerable<Booking>> GetCompletedBookingsAsync(DateTime toDate)
        {
            var query = @"
                SELECT b.* FROM bookings b
                JOIN cruises c ON b.cruise_id = c.cruise_id
                WHERE c.local_departure_time <= @ToDate";

            return await UnitOfWork.Connection.QueryAsync<Booking>(query,
                new { ToDate = toDate });
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var query = @"
                SELECT b.* FROM bookings b
                WHERE b.booking_date BETWEEN @StartDate AND @EndDate";

            return await UnitOfWork.Connection.QueryAsync<Booking>(query,
                new { StartDate = startDate, EndDate = endDate });
        }

        // Reporting queries
        public async Task<int> GetTotalBookingsCountAsync()
        {
            var query = "SELECT COUNT(*) FROM bookings";
            return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = @"
                SELECT COALESCE(SUM(c.cruise_seat_price), 0) 
                FROM bookings b
                JOIN cruises c ON b.cruise_id = c.cruise_id
                WHERE 1=1";

            if (startDate.HasValue)
                query += " AND b.booking_date >= @StartDate";

            if (endDate.HasValue)
                query += " AND b.booking_date <= @EndDate";

            return await UnitOfWork.Connection.ExecuteScalarAsync<decimal>(query,
                new { StartDate = startDate, EndDate = endDate });
        }

        // Utility methods
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var cancelledStatusId = await GetStatusIdByName("Cancelled");
            return await UpdateBookingStatusAsync(bookingId, cancelledStatusId);
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId)
        {
            var confirmedStatusId = await GetStatusIdByName("Confirmed");
            return await UpdateBookingStatusAsync(bookingId, confirmedStatusId);
        }

        public async Task<bool> ExpireOldBookingsAsync(DateTime cutoffDate)
        {
            var expiredStatusId = await GetStatusIdByName("Expired");
            var query = @"
                UPDATE bookings
                SET booking_status_id = @StatusId
                WHERE booking_expiration <= @CutoffDate
                AND booking_status_id NOT IN (
                    SELECT booking_status_id FROM booking_statuses 
                    WHERE status_name IN ('Completed', 'Cancelled', 'Expired')
                )";

            var result = await UnitOfWork.Connection.ExecuteAsync(query,
                new { StatusId = expiredStatusId, CutoffDate = cutoffDate });

            return result > 0;
        }

        private async Task<int> GetStatusIdByName(string statusName)
        {
            var query = "SELECT booking_status_id FROM booking_statuses WHERE status_name = @StatusName";
            return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query,
                new { StatusName = statusName });
        }
    }
}