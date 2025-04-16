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
using StellarPath.API.Core.Models;
using StelarPath.API.Infrastructure.Services;

public class SpaceshipServiceTests
{
    private readonly Mock<ISpaceshipRepository> _spaceshipRepoMock = new();
    private readonly Mock<IShipModelRepository> _shipModelRepoMock = new();
    private readonly Mock<ICruiseStatusService> _cruiseStatusServiceMock = new();
    private readonly Mock<ICruiseRepository> _cruiseRepoMock = new();
    private readonly Mock<ICruiseService> _cruiseServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private SpaceshipService CreateService() =>
        new(_spaceshipRepoMock.Object, _shipModelRepoMock.Object, _cruiseStatusServiceMock.Object,
            _cruiseRepoMock.Object, _cruiseServiceMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task CreateSpaceshipAsync_ShouldBeginAndCommitTransaction_WhenSuccessful()
    {
        var dto = new SpaceshipDto { SpaceshipId = 0, ModelId = 1, IsActive = true };
        _spaceshipRepoMock.Setup(r => r.AddAsync(It.IsAny<Spaceship>())).ReturnsAsync(1);

        var service = CreateService();

        var result = await service.CreateSpaceshipAsync(dto);

        Assert.Equal(1, result);
        _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task DeactivateSpaceshipAsync_ShouldReturnFalse_WhenSpaceshipNotFound()
    {
        _spaceshipRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Spaceship?)null);
        var service = CreateService();

        var (success, cancelled) = await service.DeactivateSpaceshipAsync(1);

        Assert.False(success);
        Assert.Equal(0, cancelled);
    }

    [Fact]
    public async Task DeactivateSpaceshipAsync_ShouldCancelCruises_WhenRequested()
    {
        var ship = new Spaceship { SpaceshipId = 5, IsActive = true };
        _spaceshipRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ship);
        _spaceshipRepoMock.Setup(r => r.UpdateAsync(ship)).ReturnsAsync(true);
        _cruiseServiceMock.Setup(c => c.CancelCruisesBySpaceshipIdAsync(5)).ReturnsAsync(3);

        var service = CreateService();

        var (success, cancelled) = await service.DeactivateSpaceshipAsync(5, cancelCruises: true);

        Assert.True(success);
        Assert.Equal(3, cancelled);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task ActivateSpaceshipAsync_ShouldActivate_WhenSpaceshipFound()
    {
        var ship = new Spaceship { SpaceshipId = 42, IsActive = false };
        _spaceshipRepoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(ship);
        _spaceshipRepoMock.Setup(r => r.UpdateAsync(ship)).ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ActivateSpaceshipAsync(42);

        Assert.True(result);
        Assert.True(ship.IsActive);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableSpaceshipsForTimeRangeAsync_ShouldReturnAvailableSlot_WhenNoOverlap()
    {
        var spaceship = new Spaceship { SpaceshipId = 1, ModelId = 10, IsActive = true };
        var model = new ShipModel { ModelId = 10, ModelName = "Falcon", Capacity = 100, CruiseSpeedKmph = 900 };

        _spaceshipRepoMock.Setup(r => r.GetActiveSpaceshipsAsync())
            .ReturnsAsync(new List<Spaceship> { spaceship });

        _shipModelRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(model);

        _cruiseRepoMock.Setup(r => r.GetOverlappingCruisesForSpaceshipAsync(
            spaceship.SpaceshipId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Empty<Cruise>());

        _cruiseStatusServiceMock.Setup(s => s.GetCancelledStatusIdAsync()).ReturnsAsync(99);

        var service = CreateService();
        var start = DateTime.UtcNow;
        var end = start.AddHours(5);

        var result = (await service.GetAvailableSpaceshipsForTimeRangeAsync(start, end)).ToList();

        Assert.Single(result);
        Assert.Single(result[0].AvailableTimeSlots);
    }
}
