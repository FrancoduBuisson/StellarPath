using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;

public interface IBookingStatusRepository : IRepository<BookingStatus>
{
    Task<BookingStatus?> GetByNameAsync(string statusName);
}

