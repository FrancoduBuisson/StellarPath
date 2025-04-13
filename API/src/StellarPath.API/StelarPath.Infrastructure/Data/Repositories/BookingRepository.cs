using Dapper;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Data.Repositories;

public class BookingRepository(IUnitOfWork unitOfWork) : Repository<Booking>(unitOfWork, "bookings", "booking_id"), IBookingRepository
{
    public override async Task<int> AddAsync(Booking entity)
    {
        var query = @"
            INSERT INTO bookings (google_id, cruise_id, seat_number, booking_date, booking_expiration, booking_status_id)
            VALUES (@GoogleId, @CruiseId, @SeatNumber, @BookingDate, @BookingExpiration, @BookingStatusId)
            RETURNING booking_id";

        return await UnitOfWork.Connection.ExecuteScalarAsync<int>(query, entity);
    }

    public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(string googleId)
    {
        var query = $"SELECT * FROM {TableName} WHERE google_id = @GoogleId";
        return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { GoogleId = googleId });
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCruiseAsync(int cruiseId)
    {
        var query = $"SELECT * FROM {TableName} WHERE cruise_id = @CruiseId";
        return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { CruiseId = cruiseId });
    }

    public async Task<IEnumerable<Booking>> GetActiveBookingsForCruiseAsync(int cruiseId)
    {
        var query = $@"
            SELECT * FROM {TableName} 
            WHERE cruise_id = @CruiseId 
            AND booking_status_id IN (
                SELECT booking_status_id FROM booking_statuses 
                WHERE status_name IN ('Reserved', 'Paid', 'Completed')
            )";
        return await UnitOfWork.Connection.QueryAsync<Booking>(query, new { CruiseId = cruiseId });
    }

    public override async Task<bool> UpdateAsync(Booking entity)
    {
        var query = @"
            UPDATE bookings
            SET google_id = @GoogleId,
                cruise_id = @CruiseId,
                seat_number = @SeatNumber,
                booking_date = @BookingDate,
                booking_expiration = @BookingExpiration,
                booking_status_id = @BookingStatusId
            WHERE booking_id = @BookingId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<bool> UpdateBookingStatusAsync(int bookingId, int statusId)
    {
        var query = @"
            UPDATE bookings
            SET booking_status_id = @StatusId
            WHERE booking_id = @BookingId";

        var result = await UnitOfWork.Connection.ExecuteAsync(query, new { BookingId = bookingId, StatusId = statusId });
        return result > 0;
    }
}
