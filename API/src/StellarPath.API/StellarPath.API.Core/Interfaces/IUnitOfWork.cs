using System.Data;

namespace StellarPath.API.Core.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction BeginTransaction();
    void Commit();
    void Rollback();
}
