using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface IStarSystemRepository : IRepository<StarSystem>
{
    Task<IEnumerable<StarSystem>> GetActiveStarSystemsAsync();
    Task<IEnumerable<StarSystem>> GetStarSystemsByGalaxyIdAsync(int galaxyId);
    Task<IEnumerable<StarSystem>> SearchStarSystemsAsync(
    string? name, int? galaxyId, bool? isActive);
}