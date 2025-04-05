using Dapper;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces;

namespace StelarPath.API.Infrastructure.Data.Repositories;
public abstract class Repository<T>(IUnitOfWork unitOfWork, string tableName, string idColumn) : IRepository<T> where T : class
{
    public abstract Task<int> AddAsync(T entity);
    public abstract Task<bool> UpdateAsync(T entity);

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var query = $"DELETE FROM {tableName} WHERE {idColumn} = @{id}";
        return await unitOfWork.Connection.ExecuteAsync(query, new { Id = id }) > 0;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = $"SELECT * FROM {tableName}";
        return await unitOfWork.Connection.QueryAsync<T>(query);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var query = $"SELECT * FROM {tableName} WHERE {idColumn} = @{id}";
        return await unitOfWork.Connection.QueryFirstOrDefaultAsync<T>(query, new {Id = id});
    }

    
}
