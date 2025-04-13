using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;

public interface IBookingHistoryRepository : IRepository<BookingHistory>
{
    Task<IEnumerable<BookingHistory>> GetHistoryForBookingAsync(int bookingId);

    Task<IEnumerable<BookingHistory>> SearchBookingHistoryAsync(
       int? bookingId,
       int? previousStatusId,
       int? newStatusId,
       DateTime? fromDate,
       DateTime? toDate);
}