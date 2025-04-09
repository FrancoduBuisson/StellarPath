using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;

public class ShipModelService(IShipModelRepository shipModelRepository, IUnitOfWork unitOfWork) : IShipModelService
{
    public async Task<int> CreateShipModelAsync(ShipModelDto shipModelDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var shipModel = MapToEntity(shipModelDto);
            var result = await shipModelRepository.AddAsync(shipModel);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteShipModelAsync(int id)
    {
        try
        {
            var shipModel = await shipModelRepository.GetByIdAsync(id);
            if (shipModel == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();
            var result = await shipModelRepository.DeleteAsync(id);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<ShipModelDto>> GetAllShipModelsAsync()
    {
        var shipModels = await shipModelRepository.GetAllAsync();
        return shipModels.Select(MapToDto);
    }

    public async Task<ShipModelDto?> GetShipModelByIdAsync(int id)
    {
        var shipModel = await shipModelRepository.GetByIdAsync(id);
        return shipModel != null ? MapToDto(shipModel) : null;
    }

    public async Task<IEnumerable<ShipModelDto>> SearchShipModelsAsync(
        string? name, int? minCapacity, int? maxCapacity, int? minSpeed, int? maxSpeed)
    {
        var shipModels = await shipModelRepository.SearchShipModelsAsync(
            name, minCapacity, maxCapacity, minSpeed, maxSpeed);
        return shipModels.Select(MapToDto);
    }

    public async Task<bool> UpdateShipModelAsync(ShipModelDto shipModelDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var shipModel = MapToEntity(shipModelDto);
            var result = await shipModelRepository.UpdateAsync(shipModel);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    private static ShipModelDto MapToDto(ShipModel shipModel)
    {
        return new ShipModelDto
        {
            ModelId = shipModel.ModelId,
            ModelName = shipModel.ModelName,
            Capacity = shipModel.Capacity,
            CruiseSpeedKmph = shipModel.CruiseSpeedKmph
        };
    }

    private static ShipModel MapToEntity(ShipModelDto dto)
    {
        return new ShipModel
        {
            ModelId = dto.ModelId,
            ModelName = dto.ModelName,
            Capacity = dto.Capacity,
            CruiseSpeedKmph = dto.CruiseSpeedKmph
        };
    }
}