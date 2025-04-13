using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;

public interface IBookingHistoryRepository : IRepository<BookingHistory>
{
    Task<IEnumerable<BookingHistory>> GetHistoryForBookingAsync(int bookingId);
}