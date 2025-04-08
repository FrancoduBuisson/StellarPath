using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;
public class StarSystemService(IStarSystemRepository starSystemRepository, IGalaxyRepository galaxyRepository, IUnitOfWork unitOfWork) : IStarSystemService
{
    public async Task<int> CreateStarSystemAsync(StarSystemDto starSystemDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var starSystem = MapToEntity(starSystemDto);
            var result = await starSystemRepository.AddAsync(starSystem);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeactivateStarSystemAsync(int id)
    {
        try
        {
            var starSystem = await starSystemRepository.GetByIdAsync(id);
            if (starSystem == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            starSystem.IsActive = false;
            var result = await starSystemRepository.UpdateAsync(starSystem);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> ActivateStarSystemAsync(int id)
    {
        try
        {
            var starSystem = await starSystemRepository.GetByIdAsync(id);
            if (starSystem == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            starSystem.IsActive = true;
            var result = await starSystemRepository.UpdateAsync(starSystem);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<StarSystemDto>> GetAllStarSystemsAsync()
    {
        var starSystems = await starSystemRepository.GetAllAsync();
        var starSystemDtos = new List<StarSystemDto>();

        foreach (var starSystem in starSystems)
        {
            var dto = await MapToDtoWithGalaxyNameAsync(starSystem);
            starSystemDtos.Add(dto);
        }

        return starSystemDtos;
    }

    public async Task<StarSystemDto?> GetStarSystemByIdAsync(int id)
    {
        var starSystem = await starSystemRepository.GetByIdAsync(id);
        return starSystem != null ? await MapToDtoWithGalaxyNameAsync(starSystem) : null;
    }

    public async Task<IEnumerable<StarSystemDto>> GetStarSystemsByGalaxyIdAsync(int galaxyId)
    {
        var starSystems = await starSystemRepository.GetStarSystemsByGalaxyIdAsync(galaxyId);
        var starSystemDtos = new List<StarSystemDto>();

        foreach (var starSystem in starSystems)
        {
            var dto = await MapToDtoWithGalaxyNameAsync(starSystem);
            starSystemDtos.Add(dto);
        }

        return starSystemDtos;
    }

    public async Task<bool> UpdateStarSystemAsync(StarSystemDto starSystemDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var starSystem = MapToEntity(starSystemDto);
            var result = await starSystemRepository.UpdateAsync(starSystem);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<StarSystemDto>> GetActiveStarSystemsAsync()
    {
        var activeSystems = await starSystemRepository.GetActiveStarSystemsAsync();
        var systemDtos = new List<StarSystemDto>();

        foreach (var system in activeSystems)
        {
            var dto = await MapToDtoWithGalaxyNameAsync(system);
            systemDtos.Add(dto);
        }

        return systemDtos;
    }

    public async Task<IEnumerable<StarSystemDto>> SearchStarSystemsAsync(
    string? name, int? galaxyId, string? galaxyName, bool? isActive)
    {
        if (!string.IsNullOrEmpty(galaxyName) && !galaxyId.HasValue)
        {
            var galaxies = await galaxyRepository.SearchGalaxiesAsync(name:galaxyName,null);
            var galaxy = galaxies.FirstOrDefault();
            if (galaxy != null)
            {
                galaxyId = galaxy.GalaxyId;
            }
        }

        var starSystems = await starSystemRepository.SearchStarSystemsAsync(
            name, galaxyId, isActive);

        var starSystemDtos = new List<StarSystemDto>();
        foreach (var starSystem in starSystems)
        {
            var dto = await MapToDtoWithGalaxyNameAsync(starSystem);
            starSystemDtos.Add(dto);
        }

        return starSystemDtos;
    }

    private async Task<StarSystemDto> MapToDtoWithGalaxyNameAsync(StarSystem starSystem)
    {
        var galaxy = await galaxyRepository.GetByIdAsync(starSystem.GalaxyId);

        return new StarSystemDto
        {
            SystemId = starSystem.SystemId,
            SystemName = starSystem.SystemName,
            GalaxyId = starSystem.GalaxyId,
            GalaxyName = galaxy?.GalaxyName,
            IsActive = starSystem.IsActive
        };
    }

    private static StarSystem MapToEntity(StarSystemDto dto)
    {
        return new StarSystem
        {
            SystemId = dto.SystemId,
            SystemName = dto.SystemName,
            GalaxyId = dto.GalaxyId,
            IsActive = dto.IsActive
        };
    }
}