using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StelarPath.API.Infrastructure.Services;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Models;
using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Tests.Services
{
    public class GalaxyServiceTests
    {
        private readonly Mock<IGalaxyRepository> _galaxyRepoMock = new();
        private readonly Mock<IStarSystemService> _starSystemServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly GalaxyService _galaxyService;

        public GalaxyServiceTests()
        {
            _galaxyService = new GalaxyService(
                _galaxyRepoMock.Object,
                _starSystemServiceMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ActivateGalaxyAsync_ShouldActivateGalaxy_WhenGalaxyExists()
        {
            // Arrange
            var galaxyId = 1;
            var galaxyName = "Milky Way";
            var galaxy = new Galaxy { GalaxyId = galaxyId, GalaxyName = galaxyName, IsActive = false };

            _galaxyRepoMock.Setup(repo => repo.GetByIdAsync(galaxyId))
                           .ReturnsAsync(galaxy);
            _galaxyRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<Galaxy>()))
                           .ReturnsAsync(true);

            var result = await _galaxyService.ActivateGalaxyAsync(1);

            Assert.True(result);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
            _galaxyRepoMock.Verify(repo => repo.UpdateAsync(It.Is<Galaxy>(g => g.IsActive)), Times.Once);
        }

        [Fact]
        public async Task DeactivateGalaxyAsync_ShouldDeactivateGalaxyAndStarSystems()
        {

            var galaxyId = 1;
            var galaxyName = "Milky Way";
            
            var galaxy = new Galaxy { GalaxyId = galaxyId, GalaxyName = galaxyName, IsActive = true };
            var starSystems = new List<StarSystemDto>
    {
        new() { SystemId = 101, IsActive = true },
        new() { SystemId = 102, IsActive = false }
    };

            _galaxyRepoMock.Setup(r => r.GetByIdAsync(galaxyId)).ReturnsAsync(galaxy);
            _starSystemServiceMock.Setup(s => s.GetStarSystemsByGalaxyIdAsync(galaxyId))
                                  .ReturnsAsync(starSystems);
            _galaxyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Galaxy>()))
                           .ReturnsAsync(true);

            var result = await _galaxyService.DeactivateGalaxyAsync(galaxyId);

            Assert.True(result);
            _starSystemServiceMock.Verify(s => s.DeactivateStarSystemAsync(101), Times.Once);
            _starSystemServiceMock.Verify(s => s.DeactivateStarSystemAsync(102), Times.Never);
            _galaxyRepoMock.Verify(r => r.UpdateAsync(It.Is<Galaxy>(g => g.IsActive == false)), Times.Once);
        }

        [Fact]
        public async Task ActivateGalaxyAsync_ShouldReturnFalse_WhenGalaxyDoesNotExist()
        {
            var galaxyId = 1;
            _galaxyRepoMock.Setup(repo => repo.GetByIdAsync(galaxyId))
                           .ReturnsAsync((Galaxy?)null);

            var result = await _galaxyService.ActivateGalaxyAsync(galaxyId);

            Assert.False(result);
            _galaxyRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Galaxy>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Never);
        }

        [Fact]
        public async Task CreateGalaxyAsync_ShouldAddGalaxyAndCommitTransaction()
        {
            var dto = new GalaxyDto { GalaxyId = 1, GalaxyName = "Milky Way", IsActive = true };
            _galaxyRepoMock.Setup(r => r.AddAsync(It.IsAny<Galaxy>()))
                           .ReturnsAsync(1);

            var result = await _galaxyService.CreateGalaxyAsync(dto);

            Assert.Equal(1, result);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
            _galaxyRepoMock.Verify(r => r.AddAsync(It.IsAny<Galaxy>()), Times.Once);
        }

        [Fact]
        public async Task CreateGalaxyAsync_ShouldRollbackTransaction_WhenExceptionThrown()
        {
            var dto = new GalaxyDto { GalaxyId = 1, GalaxyName = "Andromeda", IsActive = true };
            _galaxyRepoMock.Setup(r => r.AddAsync(It.IsAny<Galaxy>()))
                           .ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _galaxyService.CreateGalaxyAsync(dto));

            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public async Task GetGalaxyByIdAsync_ShouldReturnGalaxyDto_WhenGalaxyExists()
        {
            var galaxy = new Galaxy { GalaxyId = 42, GalaxyName = "Milky Way", IsActive = true};
            _galaxyRepoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(galaxy);

            var result = await _galaxyService.GetGalaxyByIdAsync(42);

            Assert.NotNull(result);
            Assert.Equal("Milky Way", result!.GalaxyName);
            Assert.Equal(42, result!.GalaxyId);
            Assert.True(result!.IsActive);
        }

        [Fact]
        public async Task UpdateGalaxyAsync_ShouldUpdateGalaxyAndCommit()
        {
            var dto = new GalaxyDto { GalaxyId = 2, GalaxyName = "Milky Way", IsActive = true };
            _galaxyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Galaxy>())).ReturnsAsync(true);

            var result = await _galaxyService.UpdateGalaxyAsync(dto);

            Assert.True(result);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task SearchGalaxiesAsync_ShouldReturnFilteredGalaxies()
        {
            var galaxies = new List<Galaxy>
                {
                    new() { GalaxyId = 1, GalaxyName = "Milky Way", IsActive = true },
                    new() { GalaxyId = 2, GalaxyName = "Galaxy B", IsActive = false },
                    new() { GalaxyId = 3, GalaxyName = "Galaxy B", IsActive = true }
                };

            _galaxyRepoMock.Setup(r => r.SearchGalaxiesAsync("Galaxy B", true)).ReturnsAsync(galaxies.FindAll(g => g.GalaxyId == 3));

            var result = await _galaxyService.SearchGalaxiesAsync("Galaxy B", true);

            Assert.Single(result);
            Assert.NotEqual("Milky Way", result.First().GalaxyName);
            Assert.Equal("Galaxy B", result.First().GalaxyName);
            Assert.Equal(3, result.First().GalaxyId);
        }


    }
}
