using StelarPath.API.Infrastructure.Data.Repositories;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;
public class DestinationService(IDestinationRepository destinationRepository, IStarSystemRepository starSystemRepository, IUnitOfWork unitOfWork) : IDestinationService
{
    public async Task<int> CreateDestinationAsync(DestinationDto destinationDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var destination = MapToEntity(destinationDto);
            var result = await destinationRepository.AddAsync(destination);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeactivateDestinationAsync(int id)
    {
        try
        {
            var destination = await destinationRepository.GetByIdAsync(id);
            if (destination == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            destination.IsActive = false;
            var result = await destinationRepository.UpdateAsync(destination);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> ActivateDestinationAsync(int id)
    {
        try
        {
            var destination = await destinationRepository.GetByIdAsync(id);
            if (destination == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            destination.IsActive = true;
            var result = await destinationRepository.UpdateAsync(destination);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<DestinationDto>> SearchDestinationsAsync(
    string? name, int? systemId, string? systemName,
    long? minDistance, long? maxDistance, bool? isActive)
    {
        if (!string.IsNullOrEmpty(systemName) && !systemId.HasValue)
        {
            var systems = await starSystemRepository.SearchStarSystemsAsync(name=systemName,null,null);
            var system = systems.FirstOrDefault();
            if (system != null)
            {
                systemId = system.SystemId;
            }
        }

        var destinations = await destinationRepository.SearchDestinationsAsync(
            name, systemId, minDistance, maxDistance, isActive);

        var destinationDtos = new List<DestinationDto>();
        foreach (var destination in destinations)
        {
            var dto = await MapToDtoWithSystemNameAsync(destination);
            destinationDtos.Add(dto);
        }

        return destinationDtos;
    }

    public async Task<IEnumerable<DestinationDto>> GetAllDestinationsAsync()
    {
        var destinations = await destinationRepository.GetAllAsync();
        var destinationDtos = new List<DestinationDto>();

        foreach (var destination in destinations)
        {
            var dto = await MapToDtoWithSystemNameAsync(destination);
            destinationDtos.Add(dto);
        }

        return destinationDtos;
    }

    public async Task<DestinationDto?> GetDestinationByIdAsync(int id)
    {
        var destination = await destinationRepository.GetByIdAsync(id);
        return destination != null ? await MapToDtoWithSystemNameAsync(destination) : null;
    }

    public async Task<IEnumerable<DestinationDto>> GetDestinationsBySystemIdAsync(int systemId)
    {
        var destinations = await destinationRepository.GetDestinationsBySystemIdAsync(systemId);
        var destinationDtos = new List<DestinationDto>();

        foreach (var destination in destinations)
        {
            var dto = await MapToDtoWithSystemNameAsync(destination);
            destinationDtos.Add(dto);
        }

        return destinationDtos;
    }

    public async Task<bool> UpdateDestinationAsync(DestinationDto destinationDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var destination = MapToEntity(destinationDto);
            var result = await destinationRepository.UpdateAsync(destination);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<DestinationDto>> GetActiveDestinationsAsync()
    {
        var activeDestinations = await destinationRepository.GetActiveDestinationsAsync();
        var destinationDtos = new List<DestinationDto>();

        foreach (var destination in activeDestinations)
        {
            var dto = await MapToDtoWithSystemNameAsync(destination);
            destinationDtos.Add(dto);
        }

        return destinationDtos;
    }

    private async Task<DestinationDto> MapToDtoWithSystemNameAsync(Destination destination)
    {
        var starSystem = await starSystemRepository.GetByIdAsync(destination.SystemId);

        return new DestinationDto
        {
            DestinationId = destination.DestinationId,
            Name = destination.Name,
            SystemId = destination.SystemId,
            SystemName = starSystem?.SystemName,
            DistanceFromEarth = destination.DistanceFromEarth,
            IsActive = destination.IsActive
        };
    }

    private static Destination MapToEntity(DestinationDto dto)
    {
        return new Destination
        {
            DestinationId = dto.DestinationId,
            Name = dto.Name,
            SystemId = dto.SystemId,
            DistanceFromEarth = dto.DistanceFromEarth,
            IsActive = dto.IsActive
        };
    }
}