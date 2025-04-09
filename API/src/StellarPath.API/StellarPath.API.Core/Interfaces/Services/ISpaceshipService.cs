using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface ISpaceshipService
{
    Task<IEnumerable<SpaceshipDto>> GetAllSpaceshipsAsync();
    Task<IEnumerable<SpaceshipDto>> GetActiveSpaceshipsAsync();
    Task<SpaceshipDto?> GetSpaceshipByIdAsync(int id);
    Task<IEnumerable<SpaceshipDto>> GetSpaceshipsByModelIdAsync(int modelId);
    Task<int> CreateSpaceshipAsync(SpaceshipDto spaceshipDto);
    Task<bool> UpdateSpaceshipAsync(SpaceshipDto spaceshipDto);
    Task<bool> DeactivateSpaceshipAsync(int id);
    Task<bool> ActivateSpaceshipAsync(int id);
    Task<bool> DeleteSpaceshipAsync(int id);
    Task<IEnumerable<SpaceshipDto>> SearchSpaceshipsAsync(
        int? modelId,
        string? modelName,
        bool? isActive);
    Task<IEnumerable<SpaceshipAvailabilityDto>> GetAvailableSpaceshipsForTimeRangeAsync(
        DateTime startTime,
        DateTime endTime);
}