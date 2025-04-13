using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsByUserAsync(string googleId);
    Task<IEnumerable<Booking>> GetBookingsByCruiseAsync(int cruiseId);
    Task<IEnumerable<Booking>> GetActiveBookingsForCruiseAsync(int cruiseId);
    Task<bool> UpdateBookingStatusAsync(int bookingId, int statusId);
}