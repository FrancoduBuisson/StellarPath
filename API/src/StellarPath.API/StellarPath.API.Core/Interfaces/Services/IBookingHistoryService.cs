using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Services
{
    public interface IBookingHistoryService
    {
        Task<BookingHistoryDto> AddHistoryRecordAsync(BookingHistoryDto historyDto);
        Task<IEnumerable<BookingHistoryDto>> GetHistoryForBookingAsync(int bookingId);
        Task<IEnumerable<BookingHistoryDto>> GetRecentStatusChangesAsync(int limit = 100);
    }
}