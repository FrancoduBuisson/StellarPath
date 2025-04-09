using StellarPath.API.Core.Models;

namespace StellarPath.API.Core.Interfaces.Repositories;
public interface ICruiseStatusRepository : IRepository<CruiseStatus>
{
    Task<CruiseStatus?> GetByNameAsync(string statusName);
}