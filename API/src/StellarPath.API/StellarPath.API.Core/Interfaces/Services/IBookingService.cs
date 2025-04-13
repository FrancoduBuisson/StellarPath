using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;

public interface IBookingService
{
    Task<int> CreateBookingAsync(CreateBookingDto bookingDto, string googleId);
    Task<BookingDto?> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(string googleId);
    Task<IEnumerable<BookingDto>> GetBookingsByCruiseAsync(int cruiseId);
    Task<bool> CancelBookingAsync(int id, string googleId);
    Task<bool> PayForBookingAsync(int id, string googleId);
    Task<IEnumerable<int>> GetAvailableSeatsForCruiseAsync(int cruiseId);
    Task<int> CancelBookingsForCruiseAsync(int cruiseId);
    Task<IEnumerable<BookingDto>> SearchBookingsAsync(SearchBookingsDto searchParams);
    Task<IEnumerable<BookingHistoryDto>> SearchBookingHistoryAsync(SearchBookingHistoryDto searchParams);

}