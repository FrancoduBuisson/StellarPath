using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories
{
    public interface IBookingHistoryRepository : IRepository<BookingHistory>
    {
        Task<IEnumerable<BookingHistory>> GetHistoryForBookingAsync(int bookingId);
        Task<BookingHistory> AddHistoryRecordAsync(BookingHistory history);
        Task<bool> UpdateHistoryRecordAsync(BookingHistory history);
        Task<int> AddStatusChangeAsync(int bookingId, int oldStatusId, int newStatusId);

    }
}
