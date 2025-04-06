using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints;
public static class GalaxyEndpoints
{
    public static WebApplication RegisterGalaxyEndpoints(this WebApplication app)
    {
        var galaxyGroup = app.MapGroup("/api/galaxies")
            .WithTags("Galaxies");

        galaxyGroup.MapGet("/", GetAllGalaxies)
            .WithName("GetAllGalaxies")
            .Produces<IEnumerable<GalaxyDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        galaxyGroup.MapGet("/{id}", GetGalaxyById)
            .WithName("GetGalaxyById")
            .Produces<GalaxyDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        galaxyGroup.MapPost("/", CreateGalaxy)
            .WithName("CreateGalaxy")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        galaxyGroup.MapPut("/{id}", UpdateGalaxy)
            .WithName("UpdateGalaxy")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        galaxyGroup.MapPatch("/{id}/deactivate", DeactivateGalaxy)
            .WithName("DeactivateGalaxy")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin"); 

        return app;
    }

    private static async Task<IResult> GetAllGalaxies(IGalaxyService galaxyService)
    {
        var galaxies = await galaxyService.GetAllGalaxiesAsync();
        return Results.Ok(galaxies);
    }

    private static async Task<IResult> GetGalaxyById(int id, IGalaxyService galaxyService)
    {
        var galaxy = await galaxyService.GetGalaxyByIdAsync(id);
        return galaxy != null ? Results.Ok(galaxy) : Results.NotFound();
    }

    private static async Task<IResult> CreateGalaxy(GalaxyDto galaxyDto, IGalaxyService galaxyService)
    {
        if (galaxyDto.GalaxyId != 0)
        {
            return Results.BadRequest("Galaxy ID should not be provided for creation");
        }

        var id = await galaxyService.CreateGalaxyAsync(galaxyDto);
        return Results.Created($"/api/galaxies/{id}", id);
    }

    private static async Task<IResult> UpdateGalaxy(int id, GalaxyDto galaxyDto, IGalaxyService galaxyService)
    {
        if (id != galaxyDto.GalaxyId)
        {
            return Results.BadRequest("ID mismatch");
        }

        var galaxy = await galaxyService.GetGalaxyByIdAsync(id);
        if (galaxy == null)
        {
            return Results.NotFound();
        }

        await galaxyService.UpdateGalaxyAsync(galaxyDto);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateGalaxy(int id, IGalaxyService galaxyService)
    {
        var galaxy = await galaxyService.GetGalaxyByIdAsync(id);
        if (galaxy == null)
        {
            return Results.NotFound();
        }

        await galaxyService.DeactivateGalaxyAsync(id);
        return Results.NoContent();
    }
}

