using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface IShipModelRepository : IRepository<ShipModel>
{
    Task<IEnumerable<ShipModel>> SearchShipModelsAsync(
        string? name,
        int? minCapacity,
        int? maxCapacity,
        int? minSpeed,
        int? maxSpeed);
}