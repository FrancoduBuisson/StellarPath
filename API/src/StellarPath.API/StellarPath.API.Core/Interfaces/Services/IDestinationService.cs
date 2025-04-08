using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IDestinationService
{
    Task<IEnumerable<DestinationDto>> GetAllDestinationsAsync();
    Task<IEnumerable<DestinationDto>> GetActiveDestinationsAsync();
    Task<DestinationDto?> GetDestinationByIdAsync(int id);
    Task<IEnumerable<DestinationDto>> GetDestinationsBySystemIdAsync(int systemId);
    Task<int> CreateDestinationAsync(DestinationDto destinationDto);
    Task<bool> UpdateDestinationAsync(DestinationDto destinationDto);
    Task<bool> DeactivateDestinationAsync(int id);
    Task<bool> ActivateDestinationAsync(int id);
    Task<IEnumerable<DestinationDto>> SearchDestinationsAsync(
    string? name, int? systemId, string? systemName,
    long? minDistance, long? maxDistance, bool? isActive);
}