using System.Data.SqlTypes;
using Npgsql;

namespace StellarPath.Services
{
    public class PostgresConnectionFactory
    {
        private readonly string _connectionString;
        public PostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;

        }

        public NpgsqlConnection Create()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }

}
