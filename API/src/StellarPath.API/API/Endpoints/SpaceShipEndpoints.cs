using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints;
public static class SpaceshipEndpoints
{
    public static WebApplication RegisterSpaceshipEndpoints(this WebApplication app)
    {
        var spaceshipGroup = app.MapGroup("/api/spaceships")
            .WithTags("Spaceships");

        spaceshipGroup.MapGet("/", GetAllSpaceships)
            .WithName("GetAllSpaceships")
            .Produces<IEnumerable<SpaceshipDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        spaceshipGroup.MapGet("/active", GetActiveSpaceships)
            .WithName("GetActiveSpaceships")
            .Produces<IEnumerable<SpaceshipDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        spaceshipGroup.MapGet("/{id}", GetSpaceshipById)
            .WithName("GetSpaceshipById")
            .Produces<SpaceshipDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        spaceshipGroup.MapGet("/model/{modelId}", GetSpaceshipsByModelId)
            .WithName("GetSpaceshipsByModelId")
            .Produces<IEnumerable<SpaceshipDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        spaceshipGroup.MapGet("/available", GetAvailableSpaceshipsForTimeRange)
            .WithName("GetAvailableSpaceshipsForTimeRange")
            .Produces<IEnumerable<SpaceshipAvailabilityDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        spaceshipGroup.MapPost("/", CreateSpaceship)
            .WithName("CreateSpaceship")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        spaceshipGroup.MapPut("/{id}", UpdateSpaceship)
            .WithName("UpdateSpaceship")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        spaceshipGroup.MapPatch("/{id}/deactivate", DeactivateSpaceship)
            .WithName("DeactivateSpaceship")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        spaceshipGroup.MapPatch("/{id}/activate", ActivateSpaceship)
            .WithName("ActivateSpaceship")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        return app;
    }

    private static async Task<IResult> GetActiveSpaceships(ISpaceshipService spaceshipService)
    {
        var spaceships = await spaceshipService.GetActiveSpaceshipsAsync();
        return Results.Ok(spaceships);
    }

    private static async Task<IResult> GetSpaceshipById(int id, ISpaceshipService spaceshipService)
    {
        var spaceship = await spaceshipService.GetSpaceshipByIdAsync(id);
        return spaceship != null ? Results.Ok(spaceship) : Results.NotFound();
    }

    private static async Task<IResult> GetSpaceshipsByModelId(int modelId, ISpaceshipService spaceshipService, IShipModelService shipModelService)
    {
        var shipModel = await shipModelService.GetShipModelByIdAsync(modelId);
        if (shipModel == null)
        {
            return Results.NotFound("Ship model not found");
        }

        var spaceships = await spaceshipService.GetSpaceshipsByModelIdAsync(modelId);
        return Results.Ok(spaceships);
    }

    private static async Task<IResult> GetAvailableSpaceshipsForTimeRange(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        ISpaceshipService spaceshipService)
    {
        if (startTime >= endTime)
        {
            return Results.BadRequest("Start time must be before end time");
        }

        var availableSpaceships = await spaceshipService.GetAvailableSpaceshipsForTimeRangeAsync(startTime, endTime);
        return Results.Ok(availableSpaceships);
    }

    private static async Task<IResult> CreateSpaceship(SpaceshipDto spaceshipDto, ISpaceshipService spaceshipService, IShipModelService shipModelService)
    {
        if (spaceshipDto.SpaceshipId != 0)
        {
            return Results.BadRequest("Spaceship ID should not be provided for creation");
        }

        var shipModel = await shipModelService.GetShipModelByIdAsync(spaceshipDto.ModelId);
        if (shipModel == null)
        {
            return Results.BadRequest($"Ship model with ID {spaceshipDto.ModelId} not found");
        }

        var id = await spaceshipService.CreateSpaceshipAsync(spaceshipDto);
        return Results.Created($"/api/spaceships/{id}", id);
    }

    private static async Task<IResult> UpdateSpaceship(int id, SpaceshipDto spaceshipDto, ISpaceshipService spaceshipService, IShipModelService shipModelService)
    {
        if (id != spaceshipDto.SpaceshipId)
        {
            return Results.BadRequest("ID mismatch");
        }

        var spaceship = await spaceshipService.GetSpaceshipByIdAsync(id);
        if (spaceship == null)
        {
            return Results.NotFound();
        }

        var shipModel = await shipModelService.GetShipModelByIdAsync(spaceshipDto.ModelId);
        if (shipModel == null)
        {
            return Results.BadRequest($"Ship model with ID {spaceshipDto.ModelId} not found");
        }

        await spaceshipService.UpdateSpaceshipAsync(spaceshipDto);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteSpaceship(int id, ISpaceshipService spaceshipService)
    {
        var spaceship = await spaceshipService.GetSpaceshipByIdAsync(id);
        if (spaceship == null)
        {
            return Results.NotFound();
        }

        await spaceshipService.DeleteSpaceshipAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateSpaceship(int id, ISpaceshipService spaceshipService)
    {
        var spaceship = await spaceshipService.GetSpaceshipByIdAsync(id);
        if (spaceship == null)
        {
            return Results.NotFound();
        }

        await spaceshipService.DeactivateSpaceshipAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> ActivateSpaceship(int id, ISpaceshipService spaceshipService)
    {
        var spaceship = await spaceshipService.GetSpaceshipByIdAsync(id);
        if (spaceship == null)
        {
            return Results.NotFound();
        }

        await spaceshipService.ActivateSpaceshipAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllSpaceships(
        [FromQuery] int? modelId,
        [FromQuery] string? modelName,
        [FromQuery] bool? isActive,
        ISpaceshipService spaceshipService)
    {
        if (modelId.HasValue || !string.IsNullOrEmpty(modelName) || isActive.HasValue)
        {
            var spaceships = await spaceshipService.SearchSpaceshipsAsync(
                modelId, modelName, isActive);
            return Results.Ok(spaceships);
        }

        var allSpaceships = await spaceshipService.GetAllSpaceshipsAsync();
        return Results.Ok(allSpaceships);
    }
}