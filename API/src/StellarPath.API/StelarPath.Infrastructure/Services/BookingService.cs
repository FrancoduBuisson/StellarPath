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
        private readonly IShipModelRepository _shipModelRepository;
        private readonly IUserProvider _userProvider;
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
            IShipModelRepository shipModelRepository,
            IUserProvider userProvider,
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
            _shipModelRepository = shipModelRepository;
            _userProvider = userProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();

            var result = new List<BookingDto>();

            foreach (var booking in bookings)
            {
                var bookingDto = await MapToDto(booking);

                if (bookingDto != null)
                {
                    result.Add(bookingDto);
                }
            }
            return result;
        }

        public async Task<BookingDto> CreateBookingAsync(int cruiseId, int seatNumber)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var bookingStatus = await _bookingStatusRepository.GetByStatusNameAsync("Reserved");

                if (bookingStatus == null) { return new BookingDto(); }

                var userId = _userProvider.GetCurrentUserId();

                var booking = new Booking
                {
                    GoogleId = userId,
                    CruiseId = cruiseId,
                    SeatNumber = seatNumber,
                    BookingDate = DateTime.UtcNow,
                    BookingExpiration = DateTime.UtcNow.AddHours(24),
                    BookingStatusId = bookingStatus.BookingStatusId
                };

                var bookingId = await _bookingRepository.AddAsync(booking);
                    
                _unitOfWork.Commit();
                return await GetBookingByIdAsync(bookingId);
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        public async Task<int> GetBookedSeatsCountForCruiseAsync(int cruiseId)
        {
            return await _bookingRepository.GetBookedSeatsCountForCruiseAsync(cruiseId);
        }

        public async Task<bool> IsSeatAvailableForCruiseAsync(int cruiseId, int seatNumber)
        {

            bool isSeatAvailable = await _bookingRepository.IsSeatAvailableForCruiseAsync(cruiseId, seatNumber);

            return isSeatAvailable;
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            try
            {

                _unitOfWork.BeginTransaction();
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                var cancelledStatus = await _bookingStatusRepository.GetByStatusNameAsync("Cancelled");
                var result = await _bookingRepository.UpdateBookingStatusAsync(bookingId, cancelledStatus.BookingStatusId);

                if (result)
                {
                    await _bookingHistoryRepository.AddStatusChangeAsync(
                        bookingId,
                        booking.BookingStatusId,
                        cancelledStatus.BookingStatusId);
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
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                var confirmedStatus = await _bookingStatusRepository.GetByStatusNameAsync("Completed");
                var result = await _bookingRepository.UpdateBookingStatusAsync(bookingId, confirmedStatus.BookingStatusId);

                if (result)
                {
                    
                    await _bookingHistoryRepository.AddStatusChangeAsync(
                        bookingId,
                        booking.BookingStatusId,
                        confirmedStatus.BookingStatusId);
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

            return await MapToDto(booking);
            
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsForUserAsync(string googleId)
        {
            var bookings = await _bookingRepository.GetBookingsForUserAsync(googleId);
            var result = new List<BookingDto>();

            foreach (var booking in bookings)
            {
                var userBooking = await GetBookingByIdAsync(booking.BookingId);

                if (userBooking != null)
                {
                    result.Add(userBooking);
                }
            }
            return result;
        }

        public async Task<IEnumerable<BookingDto>> GetActiveBookingsForUserAsync(string googleId)
        {
            var bookings = await _bookingRepository.GetActiveBookingsForUserAsync(googleId);
            var result = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var activeBooking = await GetBookingByIdAsync(booking.BookingId);

                if (activeBooking != null)
                {
                    result.Add(activeBooking);
                }
            }
            return result;
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsForCruiseAsync(int cruiseId)
        {
            var bookings = await _bookingRepository.GetBookingsForCruiseAsync(cruiseId);

            var result = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var cruiseBooking = await GetBookingByIdAsync(booking.BookingId);

                if (cruiseBooking != null) {

                    result.Add(cruiseBooking);
                }
            }

            return result;
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

            var statusIds = history
                .SelectMany(h => new[] { h.PreviousBookingStatusId, h.NewBookingStatusId })
                .Distinct()
                .ToList();

            var statuses = (await _bookingStatusRepository.GetByIdsAsync(statusIds))
                .ToDictionary(s => s.BookingStatusId, s => s.StatusName);

            return history.Select(h => new BookingHistoryDto
            {
                BookingHistoryId = h.HistoryId,
                ChangedAt = h.ChangedAt,
                PreviousStatus = statuses.GetValueOrDefault(h.PreviousBookingStatusId, "Unknown"),
                NewStatus = statuses.GetValueOrDefault(h.NewBookingStatusId, "Unknown")
            });
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

        private async Task<BookingDto> MapToDto(Booking booking)
        {
            var cruise = await _cruiseRepository.GetByIdAsync(booking.CruiseId);
            if (cruise == null) return null;
            var user = await _userRepository.GetByGoogleIdAsync(booking.GoogleId);
            var departureDest = await _destinationRepository.GetByIdAsync(cruise.DepartureDestinationId);
            var arrivalDest = await _destinationRepository.GetByIdAsync(cruise.ArrivalDestinationId);
            var spaceship = await _spaceshipRepository.GetByIdAsync(cruise.SpaceshipId);
            var status = await _bookingStatusRepository.GetByIdAsync(booking.BookingStatusId);
            var shipModel = spaceship != null ? await _shipModelRepository.GetByIdAsync(spaceship.ModelId) : null;

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
                SpaceshipModel = shipModel?.ModelName ?? string.Empty,
                SpaceshipCapacity = shipModel?.Capacity ?? 0,
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
            };
        }
    }
}