using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StelarPath.API.Infrastructure.Services;
using StellarPath.API.Core.Models;
using StelarPath.API.Infrastructure.Data.Repositories;

namespace StellarPath.API.Tests.Services
{
    public class CruiseServiceTests
    {
        private readonly Mock<ICruiseRepository> _cruiseRepo = new();
        private readonly Mock<ISpaceshipRepository> _spaceshipRepo = new();
        private readonly Mock<IShipModelRepository> _shipModelRepo = new();
        private readonly Mock<IDestinationRepository> _destinationRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IBookingHistoryRepository> _bookingHistoryRepo = new();
        private readonly Mock<IBookingStatusService> _bookingStatusService = new();
        private readonly Mock<ICruiseStatusService> _cruiseStatusService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private readonly CruiseService _cruiseService;

        public CruiseServiceTests()
        {
            _cruiseService = new CruiseService(
                _cruiseRepo.Object,
                _spaceshipRepo.Object,
                _shipModelRepo.Object,
                _destinationRepo.Object,
                _userRepo.Object,
                _bookingRepo.Object,
                _bookingHistoryRepo.Object,
                _bookingStatusService.Object,
                _cruiseStatusService.Object,
                _unitOfWork.Object);
        }

        [Fact]
        public async Task CreateCruiseAsync_CreatesCruiseSuccessfully()
        {
            // Arrange
            var dto = new CreateCruiseDto
            {
                SpaceshipId = 1,
                DepartureDestinationId = 100,
                ArrivalDestinationId = 200,
                CruiseSeatPrice = 1500,
                LocalDepartureTime = DateTime.UtcNow.AddDays(2)
            };

            _spaceshipRepo.Setup(x => x.GetByIdAsync(dto.SpaceshipId))
                          .ReturnsAsync(new Spaceship { SpaceshipId = 1, IsActive = true, ModelId = 5 });

            _destinationRepo.Setup(x => x.GetByIdAsync(dto.DepartureDestinationId))
                            .ReturnsAsync(new Destination { Name = "Pluto", DestinationId = 100, IsActive = true, DistanceFromEarth = 300 });

            _destinationRepo.Setup(x => x.GetByIdAsync(dto.ArrivalDestinationId))
                            .ReturnsAsync(new Destination { Name = "Mars", DestinationId = 200, IsActive = true, DistanceFromEarth = 500 });

            _shipModelRepo.Setup(x => x.GetByIdAsync(5))
                          .ReturnsAsync(new ShipModel { Capacity = 100, ModelName = "Audi", ModelId = 5, CruiseSpeedKmph = 100 });

            _cruiseRepo.Setup(x => x.GetOverlappingCruisesForSpaceshipAsync(
                dto.SpaceshipId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Cruise>());

            _cruiseStatusService.Setup(x => x.GetScheduledStatusIdAsync()).ReturnsAsync(1);
            _cruiseRepo.Setup(x => x.AddAsync(It.IsAny<Cruise>())).ReturnsAsync(123); // fake CruiseId

            var result = await _cruiseService.CreateCruiseAsync(dto, "google-123");

            Assert.Equal(123, result);
            _unitOfWork.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task CreateCruiseAsync_Throws_WhenSpaceshipInactive()
        {
            var dto = new CreateCruiseDto { SpaceshipId = 1 };
            _spaceshipRepo.Setup(x => x.GetByIdAsync(dto.SpaceshipId))
                          .ReturnsAsync(new Spaceship { IsActive = false });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _cruiseService.CreateCruiseAsync(dto, "user-id"));
        }

        [Fact]
        public async Task CancelCruiseAsync_CancelsCruiseSuccessfully()
        {
            int cruiseId = 42;

            var cruise = new Cruise { CruiseId = cruiseId, CruiseSeatPrice = 200, DurationMinutes = 100, LocalDepartureTime= DateTime.UtcNow, CruiseStatusId = 1 };
            _cruiseRepo.Setup(x => x.GetByIdAsync(cruiseId)).ReturnsAsync(cruise);
            _cruiseStatusService.Setup(x => x.GetScheduledStatusIdAsync()).ReturnsAsync(1);
            _cruiseStatusService.Setup(x => x.GetCancelledStatusIdAsync()).ReturnsAsync(2);
            _bookingStatusService.Setup(x => x.GetCancelledStatusIdAsync()).ReturnsAsync(3);
            _bookingStatusService.Setup(x => x.GetCompletedStatusIdAsync()).ReturnsAsync(4);

            _cruiseRepo.Setup(x => x.UpdateCruiseStatusAsync(cruiseId, 2)).ReturnsAsync(true);
            _bookingRepo.Setup(x => x.GetBookingsByCruiseAsync(cruiseId)).ReturnsAsync(new List<Booking>
            {
                new Booking { SeatNumber = 2, BookingId = 1, BookingStatusId = 5 },
                new Booking { SeatNumber = 5, BookingId = 2, BookingStatusId = 3 } // already cancelled
            });

            _bookingRepo.Setup(x => x.UpdateBookingStatusAsync(1, 3)).Returns(Task<bool>.FromResult(true));
            _bookingHistoryRepo.Setup(x => x.AddAsync(It.IsAny<BookingHistory>())).Returns(Task.FromResult(1));

            var result = await _cruiseService.CancelCruiseAsync(cruiseId);

            Assert.True(result);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task CancelCruiseAsync_ReturnsFalse_WhenCruiseNotFound()
        {
            _cruiseRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Cruise?)null);
            var result = await _cruiseService.CancelCruiseAsync(10);
            Assert.False(result);
        }


    }
}


