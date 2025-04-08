using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;

public class SpaceshipService(
    ISpaceshipRepository spaceshipRepository,
    IShipModelRepository shipModelRepository,
    IUnitOfWork unitOfWork) : ISpaceshipService
{
    public async Task<int> CreateSpaceshipAsync(SpaceshipDto spaceshipDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var spaceship = MapToEntity(spaceshipDto);
            var result = await spaceshipRepository.AddAsync(spaceship);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteSpaceshipAsync(int id)
    {
        try
        {
            var spaceship = await spaceshipRepository.GetByIdAsync(id);
            if (spaceship == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();
            var result = await spaceshipRepository.DeleteAsync(id);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeactivateSpaceshipAsync(int id)
    {
        try
        {
            var spaceship = await spaceshipRepository.GetByIdAsync(id);
            if (spaceship == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            spaceship.IsActive = false;
            var result = await spaceshipRepository.UpdateAsync(spaceship);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> ActivateSpaceshipAsync(int id)
    {
        try
        {
            var spaceship = await spaceshipRepository.GetByIdAsync(id);
            if (spaceship == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            spaceship.IsActive = true;
            var result = await spaceshipRepository.UpdateAsync(spaceship);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<SpaceshipDto>> GetAllSpaceshipsAsync()
    {
        var spaceships = await spaceshipRepository.GetAllAsync();
        var spaceshipDtos = new List<SpaceshipDto>();

        foreach (var spaceship in spaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<IEnumerable<SpaceshipDto>> GetActiveSpaceshipsAsync()
    {
        var activeSpaceships = await spaceshipRepository.GetActiveSpaceshipsAsync();
        var spaceshipDtos = new List<SpaceshipDto>();

        foreach (var spaceship in activeSpaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<SpaceshipDto?> GetSpaceshipByIdAsync(int id)
    {
        var spaceship = await spaceshipRepository.GetByIdAsync(id);
        return spaceship != null ? await MapToDtoWithModelDetailsAsync(spaceship) : null;
    }

    public async Task<IEnumerable<SpaceshipDto>> GetSpaceshipsByModelIdAsync(int modelId)
    {
        var spaceships = await spaceshipRepository.GetSpaceshipsByModelIdAsync(modelId);
        var spaceshipDtos = new List<SpaceshipDto>();

        foreach (var spaceship in spaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<bool> UpdateSpaceshipAsync(SpaceshipDto spaceshipDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var spaceship = MapToEntity(spaceshipDto);
            var result = await spaceshipRepository.UpdateAsync(spaceship);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<SpaceshipDto>> SearchSpaceshipsAsync(
        int? modelId, string? modelName, bool? isActive)
    {
        var spaceships = await spaceshipRepository.SearchSpaceshipsAsync(
            modelId, modelName, isActive);

        var spaceshipDtos = new List<SpaceshipDto>();
        foreach (var spaceship in spaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<IEnumerable<SpaceshipAvailabilityDto>> GetAvailableSpaceshipsForTimeRangeAsync(
        DateTime startTime, DateTime endTime)
    {
        // This is a placeholder implementation. In a real application, you would:
        // 1. Get all active spaceships
        // 2. Join with cruises table to find which spaceships are already booked during the requested time range
        // 3. For each available spaceship, calculate available time slots

        var activeSpaceships = await spaceshipRepository.GetActiveSpaceshipsAsync();
        var availabilityDtos = new List<SpaceshipAvailabilityDto>();

        foreach (var spaceship in activeSpaceships)
        {
            var model = await shipModelRepository.GetByIdAsync(spaceship.ModelId);
            if (model == null) continue;

            // Here you would check against the cruises table to determine real availability
            // For now, we'll just return all active spaceships as available for the full time range
            availabilityDtos.Add(new SpaceshipAvailabilityDto
            {
                SpaceshipId = spaceship.SpaceshipId,
                ModelId = model.ModelId,
                ModelName = model.ModelName,
                Capacity = model.Capacity,
                CruiseSpeedKmph = model.CruiseSpeedKmph,
                IsActive = spaceship.IsActive,
                AvailableTimeSlots = new List<TimeSlot>
                {
                    new TimeSlot { StartTime = startTime, EndTime = endTime }
                }
            });
        }

        return availabilityDtos;
    }

    private async Task<SpaceshipDto> MapToDtoWithModelDetailsAsync(Spaceship spaceship)
    {
        var model = await shipModelRepository.GetByIdAsync(spaceship.ModelId);

        return new SpaceshipDto
        {
            SpaceshipId = spaceship.SpaceshipId,
            ModelId = spaceship.ModelId,
            ModelName = model?.ModelName,
            Capacity = model?.Capacity,
            CruiseSpeedKmph = model?.CruiseSpeedKmph,
            IsActive = spaceship.IsActive
        };
    }

    private static Spaceship MapToEntity(SpaceshipDto dto)
    {
        return new Spaceship
        {
            SpaceshipId = dto.SpaceshipId,
            ModelId = dto.ModelId,
            IsActive = dto.IsActive
        };
    }
}