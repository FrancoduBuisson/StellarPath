using Dapper;
using StellarPath.Models;
using StellarPath.Services;

namespace StellarPath.Endpoints
{
    public static class GalaxiesEndpoints
    {
        public static void MapGalaxiesEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("galaxies", async (PostgresConnectionFactory postgresConnectionFactory) =>
            {


                using var connection = postgresConnectionFactory.Create();

                const string sql = "SELECT galaxy_id AS GalaxyId, galaxy_name AS GalaxyName, is_active AS IsActive FROM galaxies";

                var galaxies = await connection.QueryAsync<Galaxies>(sql,
                    new
                    {
                        galaxy_id = "GalaxyId",
                        galaxy_name = "GalaxyName",
                        is_active = "IsActive"
                    });


                return Results.Ok(galaxies);
            });
            builder.MapGet("galaxies/{id}", async (int id, PostgresConnectionFactory postgresConnectionFactory) =>
            {
                using var connection = postgresConnectionFactory.Create();

                const string sql = @"
        SELECT 
            galaxy_id AS GalaxyId, 
            galaxy_name AS GalaxyName, 
            is_active AS IsActive 
        FROM galaxies 
        WHERE galaxy_id = @Id";

                var galaxy = await connection.QuerySingleOrDefaultAsync<Galaxies>(sql, new { Id = id });

                if (galaxy is null)
                {
                    return Results.NotFound($"Galaxy with ID {id} not found.");
                }
                else
                {

                    return Results.Ok(galaxy);
                }
            });

            builder.MapPost("galaxies", async (Galaxies galaxies, PostgresConnectionFactory postgresConnectionFactory) =>
            {
                using var connection = postgresConnectionFactory.Create();

                const string sql = @"
        INSERT INTO galaxies (galaxy_name, is_active)
        VALUES (@GalaxyName, @IsActive);";

                await connection.ExecuteAsync(sql, galaxies);

                return Results.Ok();
            });

            builder.MapPut("galaxies/{id}", async (int id, Galaxies galaxies, PostgresConnectionFactory postgresConnectionFactory) =>
            {
                using var connection = postgresConnectionFactory.Create();

                galaxies.GalaxyId = id;

                const string sql = @"
                UPDATE galaxies 
                SET galaxy_name = @galaxyName,
                    is_active = @isActive
                    WHERE galaxy_id = @GalaxyId";

                await connection.ExecuteAsync(sql, galaxies);

                return Results.NoContent();
            });

        }
    }
}
