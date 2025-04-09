using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Services;
public interface ICruiseStatusService
{
    Task<IEnumerable<CruiseStatus>> GetAllStatusesAsync();
    Task<CruiseStatus?> GetStatusByIdAsync(int id);
    Task<CruiseStatus?> GetStatusByNameAsync(string name);
    Task<int> GetScheduledStatusIdAsync();
    Task<int> GetInProgressStatusIdAsync();
    Task<int> GetCompletedStatusIdAsync();
    Task<int> GetCancelledStatusIdAsync();
}