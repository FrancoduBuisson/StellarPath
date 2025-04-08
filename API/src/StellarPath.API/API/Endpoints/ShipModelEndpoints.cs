using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints;
public static class ShipModelEndpoints
{
    public static WebApplication RegisterShipModelEndpoints(this WebApplication app)
    {
        var shipModelGroup = app.MapGroup("/api/shipmodels")
            .WithTags("Ship Models");

        shipModelGroup.MapGet("/", GetAllShipModels)
            .WithName("GetAllShipModels")
            .Produces<IEnumerable<ShipModelDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        shipModelGroup.MapGet("/{id}", GetShipModelById)
            .WithName("GetShipModelById")
            .Produces<ShipModelDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        shipModelGroup.MapPost("/", CreateShipModel)
            .WithName("CreateShipModel")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        shipModelGroup.MapPut("/{id}", UpdateShipModel)
            .WithName("UpdateShipModel")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        return app;
    }

    private static async Task<IResult> GetAllShipModels(
        [FromQuery] string? name,
        [FromQuery] int? minCapacity,
        [FromQuery] int? maxCapacity,
        [FromQuery] int? minSpeed,
        [FromQuery] int? maxSpeed,
        IShipModelService shipModelService)
    {
        if (!string.IsNullOrEmpty(name) || minCapacity.HasValue ||
            maxCapacity.HasValue || minSpeed.HasValue || maxSpeed.HasValue)
        {
            var models = await shipModelService.SearchShipModelsAsync(
                name, minCapacity, maxCapacity, minSpeed, maxSpeed);
            return Results.Ok(models);
        }

        var allModels = await shipModelService.GetAllShipModelsAsync();
        return Results.Ok(allModels);
    }

    private static async Task<IResult> GetShipModelById(int id, IShipModelService shipModelService)
    {
        var shipModel = await shipModelService.GetShipModelByIdAsync(id);
        return shipModel != null ? Results.Ok(shipModel) : Results.NotFound();
    }

    private static async Task<IResult> CreateShipModel(ShipModelDto shipModelDto, IShipModelService shipModelService)
    {
        if (shipModelDto.ModelId != 0)
        {
            return Results.BadRequest("Model ID should not be provided for creation");
        }

        if (shipModelDto.Capacity <= 0)
        {
            return Results.BadRequest("Capacity must be greater than zero");
        }

        if (shipModelDto.CruiseSpeedKmph <= 0)
        {
            return Results.BadRequest("Cruise speed must be greater than zero");
        }

        var id = await shipModelService.CreateShipModelAsync(shipModelDto);
        return Results.Created($"/api/shipmodels/{id}", id);
    }

    private static async Task<IResult> UpdateShipModel(int id, ShipModelDto shipModelDto, IShipModelService shipModelService)
    {
        if (id != shipModelDto.ModelId)
        {
            return Results.BadRequest("ID mismatch");
        }

        var shipModel = await shipModelService.GetShipModelByIdAsync(id);
        if (shipModel == null)
        {
            return Results.NotFound();
        }

        if (shipModelDto.Capacity <= 0)
        {
            return Results.BadRequest("Capacity must be greater than zero");
        }

        if (shipModelDto.CruiseSpeedKmph <= 0)
        {
            return Results.BadRequest("Cruise speed must be greater than zero");
        }

        await shipModelService.UpdateShipModelAsync(shipModelDto);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteShipModel(int id, IShipModelService shipModelService)
    {
        var shipModel = await shipModelService.GetShipModelByIdAsync(id);
        if (shipModel == null)
        {
            return Results.NotFound();
        }

        await shipModelService.DeleteShipModelAsync(id);
        return Results.NoContent();
    }
}