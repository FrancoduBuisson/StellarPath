using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface IGalaxyRepository : IRepository<Galaxy>
{
    Task<IEnumerable<Galaxy>> GetActiveGalaxiesAsync();
    Task<IEnumerable<Galaxy>> SearchGalaxiesAsync(string? name, bool? isActive);
}
