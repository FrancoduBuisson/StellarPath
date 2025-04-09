using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IShipModelService
{
    Task<IEnumerable<ShipModelDto>> GetAllShipModelsAsync();
    Task<ShipModelDto?> GetShipModelByIdAsync(int id);
    Task<int> CreateShipModelAsync(ShipModelDto shipModelDto);
    Task<bool> UpdateShipModelAsync(ShipModelDto shipModelDto);
    Task<bool> DeleteShipModelAsync(int id);
    Task<IEnumerable<ShipModelDto>> SearchShipModelsAsync(
        string? name,
        int? minCapacity,
        int? maxCapacity,
        int? minSpeed,
        int? maxSpeed);
}