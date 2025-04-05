namespace StellarPath.API.Core.Interfaces.Repositories;

// Irepository to be used be register repository, used for all created repositories to support basic functionality
public interface IRepository<T> where T: class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    // returns a int because we want the pjk of the newely created item
    Task<int> AddAsync(T entity);
}
