using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface ICruiseService
{
    Task<IEnumerable<CruiseDto>> GetAllCruisesAsync();
    Task<CruiseDto?> GetCruiseByIdAsync(int id);
    Task<IEnumerable<CruiseDto>> GetCruisesBySpaceshipIdAsync(int spaceshipId);
    Task<IEnumerable<CruiseDto>> GetCruisesByStatusAsync(int statusId);
    Task<IEnumerable<CruiseDto>> GetCruisesByDepartureDestinationAsync(int destinationId);
    Task<IEnumerable<CruiseDto>> GetCruisesByArrivalDestinationAsync(int destinationId);
    Task<IEnumerable<CruiseDto>> GetCruisesBetweenDatesAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<CruiseDto>> GetCruisesCreatedByUserAsync(string googleId);
    Task<int> CreateCruiseAsync(CreateCruiseDto cruiseDto, string createdByGoogleId);
    Task<bool> CancelCruiseAsync(int id);
    Task<bool> UpdateCruiseStatusesAsync();
    Task<IEnumerable<CruiseDto>> SearchCruisesAsync(
        int? spaceshipId,
        int? departureDestinationId,
        int? arrivalDestinationId,
        DateTime? startDate,
        DateTime? endDate,
        int? statusId,
        decimal? minPrice,
        decimal? maxPrice);
}