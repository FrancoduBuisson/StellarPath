using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Models;
using StelarPath.API.Infrastructure.Services;
using Xunit;

namespace StellarPath.API.Tests.Services
{
    public class ShipModelServiceTests
    {
        private readonly Mock<IShipModelRepository> _shipModelRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly ShipModelService _service;

        public ShipModelServiceTests()
        {
            _service = new ShipModelService(_shipModelRepo.Object, _unitOfWork.Object);
        }

        private ShipModelDto SampleDto => new()
        {
            ModelId = 1,
            ModelName = "Falcon-X",
            Capacity = 42,
            CruiseSpeedKmph = 15000
        };

        private ShipModel SampleModel => new()
        {
            ModelId = 1,
            ModelName = "Falcon-X",
            Capacity = 42,
            CruiseSpeedKmph = 15000
        };

        [Fact]
        public async Task CreateShipModelAsync_AddsAndCommits()
        {
            _shipModelRepo.Setup(x => x.AddAsync(It.IsAny<ShipModel>())).ReturnsAsync(7);

            var result = await _service.CreateShipModelAsync(SampleDto);

            Assert.Equal(7, result);
            _unitOfWork.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task DeleteShipModelAsync_DeletesAndCommits_WhenFound()
        {
            _shipModelRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(SampleModel);
            _shipModelRepo.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteShipModelAsync(1);

            Assert.True(result);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task DeleteShipModelAsync_ReturnsFalse_WhenNotFound()
        {
            _shipModelRepo.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((ShipModel?)null);

            var result = await _service.DeleteShipModelAsync(99);

            Assert.False(result);
            _unitOfWork.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public async Task GetAllShipModelsAsync_ReturnsMappedDtos()
        {
            _shipModelRepo.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<ShipModel> { SampleModel });

            var result = await _service.GetAllShipModelsAsync();

            var model = result.First();
            Assert.Equal("Falcon-X", model.ModelName);
            Assert.Equal(42, model.Capacity);
        }

        [Fact]
        public async Task GetShipModelByIdAsync_ReturnsDto_WhenFound()
        {
            _shipModelRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(SampleModel);

            var result = await _service.GetShipModelByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Falcon-X", result?.ModelName);
        }

        [Fact]
        public async Task GetShipModelByIdAsync_ReturnsNull_WhenNotFound()
        {
            _shipModelRepo.Setup(x => x.GetByIdAsync(88)).ReturnsAsync((ShipModel?)null);

            var result = await _service.GetShipModelByIdAsync(88);

            Assert.Null(result);
        }

        [Fact]
        public async Task SearchShipModelsAsync_ReturnsFilteredResults()
        {
            _shipModelRepo.Setup(x =>
                x.SearchShipModelsAsync("Falcon", 10, 100, 1000, 20000))
                .ReturnsAsync(new List<ShipModel> { SampleModel });

            var result = await _service.SearchShipModelsAsync("Falcon", 10, 100, 1000, 20000);

            Assert.Single(result);
            Assert.Equal("Falcon-X", result.First().ModelName);
        }

        [Fact]
        public async Task UpdateShipModelAsync_UpdatesAndCommits()
        {
            _shipModelRepo.Setup(x => x.UpdateAsync(It.IsAny<ShipModel>())).ReturnsAsync(true);

            var result = await _service.UpdateShipModelAsync(SampleDto);

            Assert.True(result);
            _unitOfWork.Verify(x => x.Commit(), Times.Once);
        }

    }
}

