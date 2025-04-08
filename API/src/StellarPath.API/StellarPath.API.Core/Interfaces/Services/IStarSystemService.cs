using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IStarSystemService
{
    Task<IEnumerable<StarSystemDto>> GetAllStarSystemsAsync();
    Task<IEnumerable<StarSystemDto>> GetActiveStarSystemsAsync();
    Task<StarSystemDto?> GetStarSystemByIdAsync(int id);
    Task<IEnumerable<StarSystemDto>> GetStarSystemsByGalaxyIdAsync(int galaxyId);
    Task<int> CreateStarSystemAsync(StarSystemDto starSystemDto);
    Task<bool> UpdateStarSystemAsync(StarSystemDto starSystemDto);
    Task<bool> DeactivateStarSystemAsync(int id);
    Task<bool> ActivateStarSystemAsync(int id);
    Task<IEnumerable<StarSystemDto>> SearchStarSystemsAsync(
    string? name, int? galaxyId, string? galaxyName, bool? isActive);
}