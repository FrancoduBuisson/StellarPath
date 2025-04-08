using Npgsql;
using System.Data;

namespace StelarPath.API.Infrastructure.Data;
public class ConnectionFactory (string connectionString) : IConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}

public interface IConnectionFactory
{
    IDbConnection CreateConnection();
}

