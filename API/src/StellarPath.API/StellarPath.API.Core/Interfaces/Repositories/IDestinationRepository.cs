using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface IDestinationRepository : IRepository<Destination>
{
    Task<IEnumerable<Destination>> GetActiveDestinationsAsync();
    Task<IEnumerable<Destination>> GetDestinationsBySystemIdAsync(int systemId);
    Task<IEnumerable<Destination>> SearchDestinationsAsync(
    string? name, int? systemId, long? minDistance,
    long? maxDistance, bool? isActive);
}