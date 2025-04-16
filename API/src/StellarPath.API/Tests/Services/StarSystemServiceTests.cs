using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using StellarPath.API.Core.Models;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StelarPath.API.Infrastructure.Services;
using System;
using System.Linq;

public class StarSystemServiceTests
{
    private readonly Mock<IStarSystemRepository> _starSystemRepo = new();
    private readonly Mock<IGalaxyRepository> _galaxyRepo = new();
    private readonly Mock<IDestinationService> _destinationService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private StarSystemService CreateService() =>
        new(_starSystemRepo.Object, _galaxyRepo.Object, _destinationService.Object, _unitOfWork.Object);

    [Fact]
    public async Task CreateStarSystemAsync_ShouldCreateSystem_AndCommit()
    {
        var dto = new StarSystemDto { SystemId = 0, GalaxyId = 1, SystemName = "Delta", IsActive = true };
        _starSystemRepo.Setup(r => r.AddAsync(It.IsAny<StarSystem>())).ReturnsAsync(5);

        var service = CreateService();
        var result = await service.CreateStarSystemAsync(dto);

        Assert.Equal(5, result);
        _unitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
        _unitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task DeactivateStarSystemAsync_ShouldReturnFalse_IfNotFound()
    {
        _starSystemRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((StarSystem?)null);
        var service = CreateService();

        var result = await service.DeactivateStarSystemAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeactivateStarSystemAsync_ShouldDeactivateSystem_AndDestinations()
    {
        var system = new StarSystem { SystemId = 10, SystemName = "Solaris", IsActive = true };
        _starSystemRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(system);
        _destinationService.Setup(d => d.GetDestinationsBySystemIdAsync(10)).ReturnsAsync(new List<DestinationDto>
        {
            new() { DestinationId = 1, IsActive = true },
            new() { DestinationId = 2, IsActive = false }
        });

        _starSystemRepo.Setup(r => r.UpdateAsync(system)).ReturnsAsync(true);

        var service = CreateService();
        var result = await service.DeactivateStarSystemAsync(10);

        Assert.True(result);
        _destinationService.Verify(d => d.DeactivateDestinationAsync(1), Times.Once);
        _destinationService.Verify(d => d.DeactivateDestinationAsync(2), Times.Never);
        _unitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task ActivateStarSystemAsync_ShouldThrow_IfGalaxyIsInactive()
    {
        var system = new StarSystem { SystemId = 5, GalaxyId = 3, SystemName = "Solaris", IsActive = false };
        _starSystemRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(system);
        _galaxyRepo.Setup(g => g.GetByIdAsync(3)).ReturnsAsync(new Galaxy { GalaxyId = 3, GalaxyName = "Milky Way", IsActive = false });

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ActivateStarSystemAsync(5));
        Assert.Equal("Cannot activate star system because its parent galaxy is inactive.", ex.Message);
    }

    [Fact]
    public async Task ActivateStarSystemAsync_ShouldActivate_IfGalaxyIsActive()
    {
        var system = new StarSystem { SystemId = 2, SystemName = "Solaris", GalaxyId = 1, IsActive = false };
        _starSystemRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(system);
        _galaxyRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Galaxy { GalaxyId = 1, GalaxyName = "Milky Way", IsActive = true });
        _starSystemRepo.Setup(r => r.UpdateAsync(system)).ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ActivateStarSystemAsync(2);

        Assert.True(result);
        Assert.True(system.IsActive);
        _unitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    public async Task SearchStarSystemsAsync_ShouldConvertGalaxyNameToGalaxyId()
    {
        var galaxy = new Galaxy { GalaxyId = 9, GalaxyName = "Alpha" };
        _galaxyRepo.Setup(r => r.SearchGalaxiesAsync("Alpha", null)).ReturnsAsync(new List<Galaxy> { galaxy });

        _starSystemRepo.Setup(r => r.SearchStarSystemsAsync(null, 9, null)).ReturnsAsync(new List<StarSystem>
        {
            new StarSystem { SystemId = 1, GalaxyId = 9, SystemName = "TestSys", IsActive = true }
        });

        _galaxyRepo.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(galaxy);

        var service = CreateService();
        var result = (await service.SearchStarSystemsAsync(null, null, "Alpha", null)).ToList();

        Assert.Single(result);
        Assert.Equal("Alpha", result[0].GalaxyName);
    }
}

