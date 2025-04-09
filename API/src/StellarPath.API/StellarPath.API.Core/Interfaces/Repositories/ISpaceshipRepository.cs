using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface ISpaceshipRepository : IRepository<Spaceship>
{
    Task<IEnumerable<Spaceship>> GetActiveSpaceshipsAsync();
    Task<IEnumerable<Spaceship>> GetSpaceshipsByModelIdAsync(int modelId);
    Task<IEnumerable<Spaceship>> SearchSpaceshipsAsync(
        int? modelId,
        string? modelName,
        bool? isActive);
}