using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Models;
using StelarPath.API.Infrastructure.Data.Repositories;

namespace StelarPath.API.Infrastructure.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IBookingHistoryRepository bookingHistoryRepository,
    IBookingStatusService bookingStatusService,
    ICruiseService cruiseService,
    ICruiseRepository cruiseRepository,
    ISpaceshipService spaceshipService,
    IUserService userService,
    IBookingStatusRepository bookingStatusRepository,
    IUnitOfWork unitOfWork) : IBookingService
{
    private static readonly TimeSpan ReservationExpirationTime = TimeSpan.FromMinutes(30);

    public async Task<int> CreateBookingAsync(CreateBookingDto bookingDto, string googleId)
    {
        try
        {
            var cruise = await cruiseService.GetCruiseByIdAsync(bookingDto.CruiseId);
            if (cruise == null)
            {
                throw new ArgumentException($"Cruise with ID {bookingDto.CruiseId} not found");
            }

            if (cruise.CruiseStatusName != "Scheduled" && cruise.CruiseStatusName != "In Progress")
            {
                throw new InvalidOperationException($"Cannot book a cruise with status {cruise.CruiseStatusName}");
            }

            if (bookingDto.SeatNumber <= 0 || bookingDto.SeatNumber > cruise.Capacity)
            {
                throw new ArgumentException($"Invalid seat number. Must be between 1 and {cruise.Capacity}");
            }

            var availableSeats = await GetAvailableSeatsForCruiseAsync(bookingDto.CruiseId);
            if (!availableSeats.Contains(bookingDto.SeatNumber))
            {
                throw new InvalidOperationException($"Seat {bookingDto.SeatNumber} is already taken");
            }

            int statusId = await bookingStatusService.GetReservedStatusIdAsync();
            var now = DateTime.UtcNow;

            unitOfWork.BeginTransaction();

            var booking = new Booking
            {
                GoogleId = googleId,
                CruiseId = bookingDto.CruiseId,
                SeatNumber = bookingDto.SeatNumber,
                BookingDate = now,
                BookingExpiration = now.Add(ReservationExpirationTime),
                BookingStatusId = statusId
            };

            var bookingId = await bookingRepository.AddAsync(booking);

            var bookingHistory = new BookingHistory
            {
                BookingId = bookingId,
                PreviousBookingStatusId = statusId, 
                NewBookingStatusId = statusId,
                ChangedAt = now
            };

            await bookingHistoryRepository.AddAsync(bookingHistory);

            unitOfWork.Commit();
            return bookingId;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<BookingDto?> GetBookingByIdAsync(int id)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            return null;
        }

        await UpdateBookingIfExpiredAsync(booking);
        return await MapToDtoWithDetailsAsync(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(string googleId)
    {
        var bookings = await bookingRepository.GetBookingsByUserAsync(googleId);
        var bookingDtos = new List<BookingDto>();

        foreach (var booking in bookings)
        {
            await UpdateBookingIfExpiredAsync(booking);
            var dto = await MapToDtoWithDetailsAsync(booking);
            bookingDtos.Add(dto);
        }

        return bookingDtos;
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByCruiseAsync(int cruiseId)
    {
        var bookings = await bookingRepository.GetBookingsByCruiseAsync(cruiseId);
        var bookingDtos = new List<BookingDto>();

        foreach (var booking in bookings)
        {
            await UpdateBookingIfExpiredAsync(booking);
            var dto = await MapToDtoWithDetailsAsync(booking);
            bookingDtos.Add(dto);
        }

        return bookingDtos;
    }

    public async Task<bool> CancelBookingAsync(int id, string googleId)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return false;
            }

            if (booking.GoogleId != googleId)
            {
                throw new UnauthorizedAccessException("You can only cancel your own bookings");
            }

            if (booking.BookingStatusId == await bookingStatusService.GetCancelledStatusIdAsync() ||
                booking.BookingStatusId == await bookingStatusService.GetCompletedStatusIdAsync())
            {
                throw new InvalidOperationException("Cannot cancel a booking that is already cancelled or completed");
            }

            unitOfWork.BeginTransaction();

            int cancelledStatusId = await bookingStatusService.GetCancelledStatusIdAsync();

            var bookingHistory = new BookingHistory
            {
                BookingId = id,
                PreviousBookingStatusId = booking.BookingStatusId,
                NewBookingStatusId = cancelledStatusId,
                ChangedAt = DateTime.UtcNow
            };

            await bookingHistoryRepository.AddAsync(bookingHistory);

            var result = await bookingRepository.UpdateBookingStatusAsync(id, cancelledStatusId);

            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> PayForBookingAsync(int id, string googleId)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return false;
            }

            if (booking.GoogleId != googleId)
            {
                throw new UnauthorizedAccessException("You can only pay for your own bookings");
            }

            if (booking.BookingStatusId != await bookingStatusService.GetReservedStatusIdAsync())
            {
                throw new InvalidOperationException("Can only pay for bookings with 'Reserved' status");
            }

            await UpdateBookingIfExpiredAsync(booking);

            if (booking.BookingStatusId != await bookingStatusService.GetReservedStatusIdAsync())
            {
                throw new InvalidOperationException("Booking has expired and cannot be paid for");
            }

            unitOfWork.BeginTransaction();

            int paidStatusId = await bookingStatusService.GetPaidStatusIdAsync();

            var bookingHistory = new BookingHistory
            {
                BookingId = id,
                PreviousBookingStatusId = booking.BookingStatusId,
                NewBookingStatusId = paidStatusId,
                ChangedAt = DateTime.UtcNow
            };

            await bookingHistoryRepository.AddAsync(bookingHistory);

            var result = await bookingRepository.UpdateBookingStatusAsync(id, paidStatusId);

            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<int>> GetAvailableSeatsForCruiseAsync(int cruiseId)
    {
        var cruise = await cruiseService.GetCruiseByIdAsync(cruiseId);
        if (cruise == null)
        {
            throw new ArgumentException($"Cruise with ID {cruiseId} not found");
        }

        var allSeats = await cruiseRepository.GetAvailableSeatsAsync(cruiseId);

        return allSeats.ToList();
    }

    public async Task<int> CancelBookingsForCruiseAsync(int cruiseId)
    {
        try
        {
            var bookings = await bookingRepository.GetBookingsByCruiseAsync(cruiseId);

            var cancelledStatusId = await bookingStatusService.GetCancelledStatusIdAsync();
            var completedStatusId = await bookingStatusService.GetCompletedStatusIdAsync();

            var bookingsToCancel = bookings.Where(b =>
                b.BookingStatusId != cancelledStatusId &&
                b.BookingStatusId != completedStatusId).ToList();

            if (!bookingsToCancel.Any())
            {
                return 0;
            }

            unitOfWork.BeginTransaction();

            int cancelledCount = 0;
            var now = DateTime.UtcNow;

            foreach (var booking in bookingsToCancel)
            {
                var bookingHistory = new BookingHistory
                {
                    BookingId = booking.BookingId,
                    PreviousBookingStatusId = booking.BookingStatusId,
                    NewBookingStatusId = cancelledStatusId,
                    ChangedAt = now
                };

                await bookingHistoryRepository.AddAsync(bookingHistory);

                bool success = await bookingRepository.UpdateBookingStatusAsync(booking.BookingId, cancelledStatusId);
                if (success)
                {
                    cancelledCount++;
                }
            }

            unitOfWork.Commit();
            return cancelledCount;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    private async Task<BookingDto> MapToDtoWithDetailsAsync(Booking booking)
    {
        var cruise = await cruiseService.GetCruiseByIdAsync(booking.CruiseId);
        var user = await userService.GetUserByGoogleIdAsync(booking.GoogleId);
        var statusName = await bookingStatusService.GetStatusNameByIdAsync(booking.BookingStatusId);

        return new BookingDto
        {
            BookingId = booking.BookingId,
            GoogleId = booking.GoogleId,
            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
            CruiseId = booking.CruiseId,
            DepartureDestination = cruise?.DepartureDestinationName ?? "Unknown",
            ArrivalDestination = cruise?.ArrivalDestinationName ?? "Unknown",
            LocalDepartureTime = cruise?.LocalDepartureTime ?? DateTime.MinValue,
            SeatNumber = booking.SeatNumber,
            BookingDate = booking.BookingDate,
            BookingExpiration = booking.BookingExpiration,
            BookingStatusId = booking.BookingStatusId,
            BookingStatusName = statusName
        };
    }

    private async Task UpdateBookingIfExpiredAsync(Booking booking)
    {
        int reservedStatusId = await bookingStatusService.GetReservedStatusIdAsync();
        if (booking.BookingStatusId != reservedStatusId)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (booking.BookingExpiration > now)
        {
            return;
        }

        try
        {
            unitOfWork.BeginTransaction();

            int expiredStatusId = await bookingStatusService.GetExpiredStatusIdAsync();

            var bookingHistory = new BookingHistory
            {
                BookingId = booking.BookingId,
                PreviousBookingStatusId = booking.BookingStatusId,
                NewBookingStatusId = expiredStatusId,
                ChangedAt = now
            };

            await bookingHistoryRepository.AddAsync(bookingHistory);

            await bookingRepository.UpdateBookingStatusAsync(booking.BookingId, expiredStatusId);

            booking.BookingStatusId = expiredStatusId;

            unitOfWork.Commit();
        }
        catch
        {
            unitOfWork.Rollback();
        }
    }

    public async Task<IEnumerable<BookingDto>> SearchBookingsAsync(SearchBookingsDto searchParams)
    {
        int? statusId = null;

        if (!string.IsNullOrEmpty(searchParams.StatusName))
        {
            var status = await bookingStatusRepository.GetByNameAsync(searchParams.StatusName);
            statusId = status?.BookingStatusId;
        }
        else if (searchParams.BookingStatusId.HasValue)
        {
            statusId = searchParams.BookingStatusId;
        }

        var bookings = await bookingRepository.SearchBookingsAsync(
            searchParams.GoogleId,
            searchParams.CruiseId,
            statusId,
            searchParams.FromDate,
            searchParams.ToDate,
            searchParams.SeatNumber);

        var bookingDtos = new List<BookingDto>();

        foreach (var booking in bookings)
        {
            var dto = await MapToDtoWithDetailsAsync(booking);
            bookingDtos.Add(dto);
        }

        return bookingDtos;
    }

    public async Task<IEnumerable<BookingHistoryDto>> SearchBookingHistoryAsync(SearchBookingHistoryDto searchParams)
    {
        int? previousStatusId = null;
        int? newStatusId = null;

        if (!string.IsNullOrEmpty(searchParams.PreviousStatusName))
        {
            var status = await bookingStatusRepository.GetByNameAsync(searchParams.PreviousStatusName);
            previousStatusId = status?.BookingStatusId;
        }
        else if (searchParams.PreviousStatusId.HasValue)
        {
            previousStatusId = searchParams.PreviousStatusId;
        }

        if (!string.IsNullOrEmpty(searchParams.NewStatusName))
        {
            var status = await bookingStatusRepository.GetByNameAsync(searchParams.NewStatusName);
            newStatusId = status?.BookingStatusId;
        }
        else if (searchParams.NewStatusId.HasValue)
        {
            newStatusId = searchParams.NewStatusId;
        }

        var historyItems = await bookingHistoryRepository.SearchBookingHistoryAsync(
            searchParams.BookingId,
            previousStatusId,
            newStatusId,
            searchParams.FromDate,
            searchParams.ToDate);

        var historyDtos = new List<BookingHistoryDto>();

        foreach (var item in historyItems)
        {
            var previousStatus = await bookingStatusService.GetStatusNameByIdAsync(item.PreviousBookingStatusId);
            var newStatus = await bookingStatusService.GetStatusNameByIdAsync(item.NewBookingStatusId);

            historyDtos.Add(new BookingHistoryDto
            {
                HistoryId = item.HistoryId,
                BookingId = item.BookingId,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangedAt = item.ChangedAt
            });
        }

        return historyDtos;
    }
}