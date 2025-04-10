using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StelarPath.API.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingHistoryRepository _bookingHistoryRepository;
        private readonly ICruiseRepository _cruiseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly ISpaceshipRepository _spaceshipRepository;
        private readonly IBookingStatusRepository _bookingStatusRepository;
        private readonly IStarSystemRepository _starSystemRepository;
        private readonly IGalaxyRepository _galaxyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(
            IBookingRepository bookingRepository,
            IBookingHistoryRepository bookingHistoryRepository,
            ICruiseRepository cruiseRepository,
            IUserRepository userRepository,
            IDestinationRepository destinationRepository,
            ISpaceshipRepository spaceshipRepository,
            IBookingStatusRepository bookingStatusRepository,
            IStarSystemRepository starSystemRepository,
            IGalaxyRepository galaxyRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingHistoryRepository = bookingHistoryRepository;
            _cruiseRepository = cruiseRepository;
            _userRepository = userRepository;
            _destinationRepository = destinationRepository;
            _spaceshipRepository = spaceshipRepository;
            _bookingStatusRepository = bookingStatusRepository;
            _starSystemRepository = starSystemRepository;
            _galaxyRepository = galaxyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingDto> CreateBookingAsync(BookingDto bookingDto)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var booking = new Booking
                {
                    GoogleId = bookingDto.UserGoogleId,
                    CruiseId = bookingDto.CruiseId,
                    SeatNumber = bookingDto.SeatNumber,
                    BookingDate = DateTime.UtcNow,
                    BookingExpiration = DateTime.UtcNow.AddHours(24),
                    BookingStatusId = 1 // Pending
                };
                //check seat and return available seats if not found
                //check cruis Status

                var bookingId = await _bookingRepository.AddAsync(booking);
                await _bookingHistoryRepository.AddStatusChangeAsync(bookingId, 0, booking.BookingStatusId);

                _unitOfWork.Commit();
                return await GetBookingByIdAsync(bookingId);
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var cancelledStatusId = await _bookingStatusRepository.GetStatusIdByName("Cancelled");
                var result = await _bookingRepository.UpdateBookingStatusAsync(bookingId, cancelledStatusId);

                if (result)
                {
                    var booking = await _bookingRepository.GetByIdAsync(bookingId);
                    await _bookingHistoryRepository.AddStatusChangeAsync(
                        bookingId,
                        booking.BookingStatusId,
                        cancelledStatusId);
                }

                _unitOfWork.Commit();
                return result;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var confirmedStatusId = await _bookingStatusRepository.GetStatusIdByName("Confirmed");
                var result = await _bookingRepository.UpdateBookingStatusAsync(bookingId, confirmedStatusId);

                if (result)
                {
                    var booking = await _bookingRepository.GetByIdAsync(bookingId);
                    await _bookingHistoryRepository.AddStatusChangeAsync(
                        bookingId,
                        booking.BookingStatusId,
                        confirmedStatusId);
                }

                _unitOfWork.Commit();
                return result;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return null;

            var cruise = await _cruiseRepository.GetByIdAsync(booking.CruiseId);
            if (cruise == null) return null;

            var user = await _userRepository.GetByGoogleIdAsync(booking.GoogleId);
            var departureDest = await _destinationRepository.GetByIdAsync(cruise.DepartureDestinationId);
            var arrivalDest = await _destinationRepository.GetByIdAsync(cruise.ArrivalDestinationId);
            var spaceship = await _spaceshipRepository.GetByIdAsync(cruise.SpaceshipId);
            var status = await _bookingStatusRepository.GetByIdAsync(booking.BookingStatusId);
            var history = await _bookingHistoryRepository.GetHistoryForBookingAsync(bookingId);

            return new BookingDto
            {
                BookingId = booking.BookingId,
                UserGoogleId = booking.GoogleId,
                UserEmail = user?.Email ?? string.Empty,
                UserFullName = $"{user?.FirstName} {user?.LastName}",
                CruiseId = booking.CruiseId,
                DepartureTime = cruise.LocalDepartureTime,
                DurationMinutes = cruise.DurationMinutes,
                SeatPrice = cruise.CruiseSeatPrice,
                SpaceshipModel = spaceship?.ModelName ?? string.Empty,
                SpaceshipCapacity = spaceship?.Capacity ?? 0,
                DepartureDestination = departureDest?.Name ?? string.Empty,
                DepartureStarSystem = await GetStarSystemName(departureDest?.SystemId),
                DepartureGalaxy = await GetGalaxyName(departureDest?.SystemId),
                ArrivalDestination = arrivalDest?.Name ?? string.Empty,
                ArrivalStarSystem = await GetStarSystemName(arrivalDest?.SystemId),
                ArrivalGalaxy = await GetGalaxyName(arrivalDest?.SystemId),
                SeatNumber = booking.SeatNumber,
                BookingDate = booking.BookingDate,
                BookingExpiration = booking.BookingExpiration,
                BookingStatus = status?.StatusName ?? string.Empty,
                BookingStatusId = booking.BookingStatusId,
                History = (await Task.WhenAll(history.Select(async h =>
                {
                    var prevStatus = await _bookingStatusRepository.GetByIdAsync(h.PreviousBookingStatusId);
                    var newStatus = await _bookingStatusRepository.GetByIdAsync(h.NewBookingStatusId);
                    return new BookingHistoryDto
                    {
                        BookingHistoryId = h.HistoryId,
                        ChangedAt = h.ChangedAt,
                        PreviousStatus = prevStatus?.StatusName ?? "Unknown",
                        NewStatus = newStatus?.StatusName ?? "Unknown"
                    };
                }))).ToList()
            };
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsForUserAsync(string googleId)
        {
            var bookings = await _bookingRepository.GetBookingsForUserAsync(googleId);
            return await Task.WhenAll(bookings.Select(async b => await GetBookingByIdAsync(b.BookingId)));
        }

        public async Task<IEnumerable<BookingDto>> GetActiveBookingsForUserAsync(string googleId)
        {
            var bookings = await _bookingRepository.GetActiveBookingsForUserAsync(googleId);
            return await Task.WhenAll(bookings.Select(async b => await GetBookingByIdAsync(b.BookingId)));
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsForCruiseAsync(int cruiseId)
        {
            var bookings = await _bookingRepository.GetBookingsForCruiseAsync(cruiseId);
            return await Task.WhenAll(bookings.Select(async b => await GetBookingByIdAsync(b.BookingId)));
        }

        public async Task<bool> UpdateBookingAsync(BookingDto bookingDto)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var booking = MapToEntity(bookingDto);
                var result = await _bookingRepository.UpdateAsync(booking);
                _unitOfWork.Commit();
                return result;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<bool> ExpireOldBookingsAsync(DateTime cutoffDate)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var result = await _bookingRepository.ExpireOldBookingsAsync(cutoffDate);
                _unitOfWork.Commit();
                return result;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<BookingHistoryDto>> GetBookingHistoryAsync(int bookingId)
        {
            var history = await _bookingHistoryRepository.GetHistoryForBookingAsync(bookingId);
            return await Task.WhenAll(history.Select(async h =>
            {
                var prevStatus = await _bookingStatusRepository.GetByIdAsync(h.PreviousBookingStatusId);
                var newStatus = await _bookingStatusRepository.GetByIdAsync(h.NewBookingStatusId);
                return new BookingHistoryDto
                {
                    BookingHistoryId = h.HistoryId,
                    ChangedAt = h.ChangedAt,
                    PreviousStatus = prevStatus?.StatusName ?? "Unknown",
                    NewStatus = newStatus?.StatusName ?? "Unknown"
                };
            }));
        }

        private async Task<string> GetStarSystemName(int? systemId)
        {
            if (!systemId.HasValue) return string.Empty;
            var system = await _starSystemRepository.GetByIdAsync(systemId.Value);
            return system?.SystemName ?? string.Empty;
        }

        private async Task<string> GetGalaxyName(int? systemId)
        {
            if (!systemId.HasValue) return string.Empty;
            var system = await _starSystemRepository.GetByIdAsync(systemId.Value);
            if (system == null) return string.Empty;
            var galaxy = await _galaxyRepository.GetByIdAsync(system.GalaxyId);
            return galaxy?.GalaxyName ?? string.Empty;
        }

        private static Booking MapToEntity(BookingDto dto)
        {
            return new Booking
            {
                BookingId = dto.BookingId,
                GoogleId = dto.UserGoogleId,
                CruiseId = dto.CruiseId,
                SeatNumber = dto.SeatNumber,
                BookingDate = dto.BookingDate,
                BookingExpiration = dto.BookingExpiration,
                BookingStatusId = dto.BookingStatusId
            };
        }
    }
}