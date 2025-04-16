using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Models;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Interfaces;
using StelarPath.API.Infrastructure.Services;
using Xunit;

namespace StellarPath.API.Tests.Services
{
    public class DestinationServiceTests
    {
        private readonly Mock<IDestinationRepository> _destinationRepo = new();
        private readonly Mock<ICruiseStatusService> _cruiseStatusService = new();
        private readonly Mock<ICruiseRepository> _cruiseRepo = new();
        private readonly Mock<IStarSystemRepository> _starSystemRepo = new();
        private readonly Mock<IGalaxyRepository> _galaxyRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private readonly DestinationService _service;

        public DestinationServiceTests()
        {
            _service = new DestinationService(
                _destinationRepo.Object,
                _cruiseStatusService.Object,
                _cruiseRepo.Object,
                _starSystemRepo.Object,
                _galaxyRepo.Object,
                _unitOfWork.Object);
        }

        private DestinationDto CreateSampleDto() => new()
        {
            DestinationId = 1,
            Name = "Tatooine",
            SystemId = 101,
            DistanceFromEarth = 123456,
            IsActive = true
        };

        private Destination CreateSampleDestination() => new()
        {
            DestinationId = 1,
            Name = "Tatooine",
            SystemId = 101,
            DistanceFromEarth = 123456,
            IsActive = true
        };

        [Fact]
        public async Task CreateDestinationAsync_AddsDestination_AndCommitsTransaction()
        {
            var dto = CreateSampleDto();
            _destinationRepo.Setup(x => x.AddAsync(It.IsAny<Destination>())).ReturnsAsync(123);

            var result = await _service.CreateDestinationAsync(dto);

            Assert.Equal(123, result);
            _unitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task DeactivateDestinationAsync_UpdatesStatus_AndCancelsCruises()
        {
            var id = 1;
            var destination = CreateSampleDestination();
            _destinationRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(destination);
            _cruiseStatusService.Setup(x => x.GetScheduledStatusIdAsync()).ReturnsAsync(1);
            _cruiseStatusService.Setup(x => x.GetCancelledStatusIdAsync()).ReturnsAsync(2);

            var cruiseList = new List<Cruise>
            {
                new() { CruiseId = 1, CruiseSeatPrice = 1000, DurationMinutes = 200, LocalDepartureTime= DateTime.UtcNow, CruiseStatusId = 1 }
            };

            _cruiseRepo.Setup(x => x.GetCruisesByDepartureDestinationAsync(id)).ReturnsAsync(cruiseList);
            _cruiseRepo.Setup(x => x.GetCruisesByArrivalDestinationAsync(id)).ReturnsAsync(new List<Cruise>());
            _destinationRepo.Setup(x => x.UpdateAsync(It.IsAny<Destination>())).ReturnsAsync(true);

            var result = await _service.DeactivateDestinationAsync(id);

            Assert.True(result);
            _cruiseRepo.Verify(x => x.UpdateCruiseStatusAsync(1, 2), Times.Once);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task DeactivateDestinationAsync_ReturnsFalse_IfDestinationNotFound()
        {
            _destinationRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Destination?)null);

            var result = await _service.DeactivateDestinationAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task ActivateDestinationAsync_Succeeds_WhenAllValid()
        {
            var destination = CreateSampleDestination();
            destination.IsActive = false;

            _destinationRepo.Setup(x => x.GetByIdAsync(destination.DestinationId)).ReturnsAsync(destination);
            _starSystemRepo.Setup(x => x.GetByIdAsync(destination.SystemId))
                .ReturnsAsync(new StarSystem { SystemId = 101, SystemName = "Sample System", GalaxyId = 5, IsActive = true });

            _galaxyRepo.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new Galaxy { GalaxyId = 5, GalaxyName = "Milky Way", IsActive = true });

            _destinationRepo.Setup(x => x.UpdateAsync(It.IsAny<Destination>())).ReturnsAsync(true);

            var result = await _service.ActivateDestinationAsync(destination.DestinationId);

            Assert.True(result);
            _unitOfWork.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task UpdateDestinationAsync_UpdatesAndCommits()
        {
            var dto = CreateSampleDto();
            _destinationRepo.Setup(x => x.UpdateAsync(It.IsAny<Destination>())).ReturnsAsync(true);

            var result = await _service.UpdateDestinationAsync(dto);

            Assert.True(result);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        //[Fact]
        //public async Task SearchDestinationsAsync_ResolvesSystemNameToId()
        //{
        //    var starSystems = new List<StarSystem>
        //    {
        //        new() { SystemId = 101, SystemName = "Solaris", IsActive = true }
        //    };

        //    var destinations = new List<Destination>
        //    {
        //        CreateSampleDestination()
        //    };

        //    _starSystemRepo.Setup(x => x.SearchStarSystemsAsync( "Solaris", null, null))
        //        .ReturnsAsync(starSystems);

        //    _destinationRepo.Setup(x => x.SearchDestinationsAsync(null, 101, null, null, null))
        //        .ReturnsAsync(destinations);

        //    var result = await _service.SearchDestinationsAsync(null, null, "Solaris", null, null, null);
        //    Console.WriteLine(result.Count());
        //    Assert.Single(result);
        //    Assert.Equal("Tatooine", result.First().Name);
        //}



    }
}


