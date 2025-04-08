using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints;
public static class StarSystemEndpoints
{
    public static WebApplication RegisterStarSystemEndpoints(this WebApplication app)
    {
        var starSystemGroup = app.MapGroup("/api/starsystems")
            .WithTags("Star Systems");

        starSystemGroup.MapGet("/", GetAllStarSystems)
            .WithName("GetAllStarSystems")
            .Produces<IEnumerable<StarSystemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        starSystemGroup.MapGet("/active", GetActiveStarSystems)
            .WithName("GetActiveStarSystems")
            .Produces<IEnumerable<StarSystemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        starSystemGroup.MapGet("/{id}", GetStarSystemById)
            .WithName("GetStarSystemById")
            .Produces<StarSystemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        starSystemGroup.MapGet("/galaxy/{galaxyId}", GetStarSystemsByGalaxyId)
            .WithName("GetStarSystemsByGalaxyId")
            .Produces<IEnumerable<StarSystemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        starSystemGroup.MapPost("/", CreateStarSystem)
            .WithName("CreateStarSystem")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        starSystemGroup.MapPut("/{id}", UpdateStarSystem)
            .WithName("UpdateStarSystem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        starSystemGroup.MapPatch("/{id}/deactivate", DeactivateStarSystem)
            .WithName("DeactivateStarSystem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        starSystemGroup.MapPatch("/{id}/activate", ActivateStarSystem)
            .WithName("ActivateStarSystem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");


        return app;
    }


    private static async Task<IResult> GetActiveStarSystems(IStarSystemService starSystemService)
    {
        var starSystems = await starSystemService.GetActiveStarSystemsAsync();
        return Results.Ok(starSystems);
    }

    private static async Task<IResult> GetStarSystemById(int id, IStarSystemService starSystemService)
    {
        var starSystem = await starSystemService.GetStarSystemByIdAsync(id);
        return starSystem != null ? Results.Ok(starSystem) : Results.NotFound();
    }

    private static async Task<IResult> GetStarSystemsByGalaxyId(int galaxyId, IStarSystemService starSystemService, IGalaxyService galaxyService)
    {
        var galaxy = await galaxyService.GetGalaxyByIdAsync(galaxyId);
        if (galaxy == null)
        {
            return Results.NotFound("Galaxy not found");
        }

        var starSystems = await starSystemService.GetStarSystemsByGalaxyIdAsync(galaxyId);
        return Results.Ok(starSystems);
    }

    private static async Task<IResult> CreateStarSystem(StarSystemDto starSystemDto, IStarSystemService starSystemService, IGalaxyService galaxyService)
    {
        if (starSystemDto.SystemId != 0)
        {
            return Results.BadRequest("System ID should not be provided for creation");
        }

        var galaxy = await galaxyService.GetGalaxyByIdAsync(starSystemDto.GalaxyId);
        if (galaxy == null)
        {
            return Results.BadRequest($"Galaxy with ID {starSystemDto.GalaxyId} not found");
        }

        var id = await starSystemService.CreateStarSystemAsync(starSystemDto);
        return Results.Created($"/api/starsystems/{id}", id);
    }

    private static async Task<IResult> UpdateStarSystem(int id, StarSystemDto starSystemDto, IStarSystemService starSystemService, IGalaxyService galaxyService)
    {
        if (id != starSystemDto.SystemId)
        {
            return Results.BadRequest("ID mismatch");
        }

        var starSystem = await starSystemService.GetStarSystemByIdAsync(id);
        if (starSystem == null)
        {
            return Results.NotFound();
        }

        var galaxy = await galaxyService.GetGalaxyByIdAsync(starSystemDto.GalaxyId);
        if (galaxy == null)
        {
            return Results.BadRequest($"Galaxy with ID {starSystemDto.GalaxyId} not found");
        }

        await starSystemService.UpdateStarSystemAsync(starSystemDto);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateStarSystem(int id, IStarSystemService starSystemService)
    {
        var starSystem = await starSystemService.GetStarSystemByIdAsync(id);
        if (starSystem == null)
        {
            return Results.NotFound();
        }

        await starSystemService.DeactivateStarSystemAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> ActivateStarSystem(int id, IStarSystemService starSystemService)
    {
        var starSystem = await starSystemService.GetStarSystemByIdAsync(id);
        if (starSystem == null)
        {
            return Results.NotFound();
        }

        await starSystemService.ActivateStarSystemAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllStarSystems(
    [FromQuery] string? name,
    [FromQuery] int? galaxyId,
    [FromQuery] string? galaxyName,
    [FromQuery] bool? isActive,
    IStarSystemService starSystemService)
    {
        if (!string.IsNullOrEmpty(name) || galaxyId.HasValue ||
            !string.IsNullOrEmpty(galaxyName) || isActive.HasValue)
        {
            var starSystems = await starSystemService.SearchStarSystemsAsync(
                name, galaxyId, galaxyName, isActive);
            return Results.Ok(starSystems);
        }

        var allStarSystems = await starSystemService.GetAllStarSystemsAsync();
        return Results.Ok(allStarSystems);
    }
}