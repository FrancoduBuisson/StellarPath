using Dapper;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces;

namespace StelarPath.API.Infrastructure.Data.Repositories;
public abstract class Repository<T> : IRepository<T> where T : class
{

    protected readonly IUnitOfWork UnitOfWork;
    protected readonly string TableName;
    protected readonly string IdColumn;

    protected Repository(IUnitOfWork unitOfWork, string tableName, string idColumn)
    {
        UnitOfWork = unitOfWork;
        TableName = tableName;
        IdColumn = idColumn;
    }

    public abstract Task<int> AddAsync(T entity);
    public abstract Task<bool> UpdateAsync(T entity);

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var query = $"DELETE FROM {TableName} WHERE {IdColumn} = @Id";
        return await UnitOfWork.Connection.ExecuteAsync(query, new { Id = id }) > 0;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = $"SELECT * FROM {TableName}";
        return await UnitOfWork.Connection.QueryAsync<T>(query);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var query = $"SELECT * FROM {TableName} WHERE {IdColumn} = @{id}";
        return await UnitOfWork.Connection.QueryFirstOrDefaultAsync<T>(query, new {Id = id});
    }

    
}
