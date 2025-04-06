
using System.Data;
using System.Data.Common;
using StellarPath.API.Core.Interfaces;

namespace StelarPath.API.Infrastructure.Data;
public class UnitOfWork (IConnectionFactory connectionFactory) : IUnitOfWork
{

    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public IDbConnection Connection { 
        
        get
        {
            _connection ??= connectionFactory.CreateConnection();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;

        }
    }

    public IDbTransaction BeginTransaction()
    {
        if (_transaction != null)
        {
            return _transaction;
        }

        _transaction = Connection.BeginTransaction();
        return _transaction;
    }

    public void Commit()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Rollback()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }

            _transaction = null;
            _connection = null;
            _disposed = true;
        }
    }
}

