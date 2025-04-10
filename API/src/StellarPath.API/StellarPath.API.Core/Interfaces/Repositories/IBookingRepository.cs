using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {

        // User-specific queries
        Task<IEnumerable<Booking>> GetBookingsForUserAsync(string googleId);
        Task<IEnumerable<Booking>> GetActiveBookingsForUserAsync(string googleId);
        Task<IEnumerable<Booking>> GetExpiredBookingsForUserAsync(string googleId);
        Task<IEnumerable<Booking>> GetBookingsByStatusForUserAsync(string googleId, int bookingStatusId);

        // Cruise-specific queries
        Task<IEnumerable<Booking>> GetBookingsForCruiseAsync(int cruiseId);
        Task<int> GetBookedSeatsCountForCruiseAsync(int cruiseId);
        Task<bool> IsSeatAvailableForCruiseAsync(int cruiseId, int seatNumber);

        // Booking status operations
        Task<bool> UpdateBookingStatusAsync(int bookingId, int newStatusId);
        Task<IEnumerable<BookingHistory>> GetBookingHistoryAsync(int bookingId);

        // Extras
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(DateTime fromDate);
        Task<IEnumerable<Booking>> GetCompletedBookingsAsync(DateTime toDate);
        Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Reporting queries
        Task<int> GetTotalBookingsCountAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Utility methods
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> ConfirmBookingAsync(int bookingId);
        Task<bool> ExpireOldBookingsAsync(DateTime cutoffDate);
    }
}