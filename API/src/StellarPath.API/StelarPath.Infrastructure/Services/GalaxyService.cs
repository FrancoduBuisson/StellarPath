using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;
public class GalaxyService(IGalaxyRepository galaxyRepository, IUnitOfWork unitOfWork) : IGalaxyService
{
    public async Task<int> CreateGalaxyAsync(GalaxyDto galaxyDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var galaxy = MapToEntity(galaxyDto);
            var result = await galaxyRepository.AddAsync(galaxy);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> ActivateGalaxyAsync(int id)
    {
        try
        {
            var galaxy = await galaxyRepository.GetByIdAsync(id);
            if (galaxy == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            galaxy.IsActive = true;
            var result = await galaxyRepository.UpdateAsync(galaxy);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<GalaxyDto>> SearchGalaxiesAsync(string? name, bool? isActive)
    {
        var galaxies = await galaxyRepository.SearchGalaxiesAsync(name, isActive);
        return galaxies.Select(MapToDto);
    }

    public async Task<bool> DeactivateGalaxyAsync(int id)
    {
        try
        {
            var galaxy = await galaxyRepository.GetByIdAsync(id);
            if (galaxy == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            galaxy.IsActive = false;
            var result = await galaxyRepository.UpdateAsync(galaxy);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<GalaxyDto>> GetAllGalaxiesAsync()
    {
        var galaxies = await galaxyRepository.GetAllAsync();
        return galaxies.Select(MapToDto);
    }

    public async Task<GalaxyDto?> GetGalaxyByIdAsync(int id)
    {
        var galaxy = await galaxyRepository.GetByIdAsync(id);
        return galaxy != null ? MapToDto(galaxy) : null;
    }

    public async Task<bool> UpdateGalaxyAsync(GalaxyDto galaxyDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var galaxy = MapToEntity(galaxyDto);
            var result = await galaxyRepository.UpdateAsync(galaxy);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<GalaxyDto>> GetActiveGalaxiesAsync()
    {
        var galaxies = await galaxyRepository.GetActiveGalaxiesAsync();
        return galaxies.Select(MapToDto);
    }

    private static GalaxyDto MapToDto(Galaxy galaxy)
    {
        return new GalaxyDto
        {
            GalaxyId = galaxy.GalaxyId,
            GalaxyName = galaxy.GalaxyName,
            IsActive = galaxy.IsActive
        };
    }

    private static Galaxy MapToEntity(GalaxyDto dto)
    {
        return new Galaxy
        {
            GalaxyId = dto.GalaxyId,
            GalaxyName = dto.GalaxyName,
            IsActive = dto.IsActive
        };
    }

}

