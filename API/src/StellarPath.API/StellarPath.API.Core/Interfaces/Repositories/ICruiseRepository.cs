using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface ICruiseRepository : IRepository<Cruise>
{
    Task<IEnumerable<Cruise>> GetCruisesBySpaceshipIdAsync(int spaceshipId);
    Task<IEnumerable<Cruise>> GetCruisesByStatusAsync(int statusId);
    Task<IEnumerable<Cruise>> GetCruisesByDepartureDestinationAsync(int destinationId);
    Task<IEnumerable<Cruise>> GetCruisesByArrivalDestinationAsync(int destinationId);
    Task<IEnumerable<Cruise>> GetCruisesBetweenDatesAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Cruise>> GetCruisesCreatedByUserAsync(string googleId);
    Task<IEnumerable<Cruise>> SearchCruisesAsync(
        int? spaceshipId,
        int? departureDestinationId,
        int? arrivalDestinationId,
        DateTime? startDate,
        DateTime? endDate,
        int? statusId,
        decimal? minPrice,
        decimal? maxPrice);
    Task<IEnumerable<Cruise>> GetOverlappingCruisesForSpaceshipAsync(int spaceshipId, DateTime startTime, DateTime endTime);
    Task<bool> UpdateCruiseStatusAsync(int cruiseId, int statusId);
}