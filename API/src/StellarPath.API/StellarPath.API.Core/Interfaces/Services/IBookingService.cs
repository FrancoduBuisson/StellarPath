using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(BookingDto bookingDto);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> ConfirmBookingAsync(int bookingId);
        Task<BookingDto?> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<BookingDto>> GetBookingsForUserAsync(string googleId);
        Task<IEnumerable<BookingDto>> GetActiveBookingsForUserAsync(string googleId);
        Task<IEnumerable<BookingDto>> GetBookingsForCruiseAsync(int cruiseId);
        Task<bool> UpdateBookingAsync(BookingDto bookingDto);
        Task<bool> ExpireOldBookingsAsync(DateTime cutoffDate);
        Task<IEnumerable<BookingHistoryDto>> GetBookingHistoryAsync(int bookingId);
    }
}