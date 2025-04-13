using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories
{
    public interface IBookingStatusRepository : IRepository<BookingStatus>
    {
        Task<BookingStatus?> GetByStatusNameAsync(string statusName);
        Task<IEnumerable<BookingStatus>> GetActiveStatusesAsync();
        Task<IEnumerable<BookingStatus>> GetInactiveStatusesAsync();
        Task<IEnumerable<BookingStatus>> GetByIdsAsync(IEnumerable<int> ids);
    }
}