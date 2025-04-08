using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints;
public static class DestinationEndpoints
{
    public static WebApplication RegisterDestinationEndpoints(this WebApplication app)
    {
        var destinationGroup = app.MapGroup("/api/destinations")
            .WithTags("Destinations");

        destinationGroup.MapGet("/", GetAllDestinations)
            .WithName("GetAllDestinations")
            .Produces<IEnumerable<DestinationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        destinationGroup.MapGet("/active", GetActiveDestinations)
            .WithName("GetActiveDestinations")
            .Produces<IEnumerable<DestinationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        destinationGroup.MapGet("/{id}", GetDestinationById)
            .WithName("GetDestinationById")
            .Produces<DestinationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        destinationGroup.MapGet("/system/{systemId}", GetDestinationsBySystemId)
            .WithName("GetDestinationsBySystemId")
            .Produces<IEnumerable<DestinationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        destinationGroup.MapPost("/", CreateDestination)
            .WithName("CreateDestination")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        destinationGroup.MapPut("/{id}", UpdateDestination)
            .WithName("UpdateDestination")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        destinationGroup.MapPatch("/{id}/deactivate", DeactivateDestination)
            .WithName("DeactivateDestination")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        destinationGroup.MapPatch("/{id}/activate", ActivateDestination)
            .WithName("ActivateDestination")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("Admin");

        return app;
    }


    private static async Task<IResult> GetActiveDestinations(IDestinationService destinationService)
    {
        var destinations = await destinationService.GetActiveDestinationsAsync();
        return Results.Ok(destinations);
    }

    private static async Task<IResult> GetDestinationById(int id, IDestinationService destinationService)
    {
        var destination = await destinationService.GetDestinationByIdAsync(id);
        return destination != null ? Results.Ok(destination) : Results.NotFound();
    }

    private static async Task<IResult> GetDestinationsBySystemId(int systemId, IDestinationService destinationService, IStarSystemService starSystemService)
    {
        var starSystem = await starSystemService.GetStarSystemByIdAsync(systemId);
        if (starSystem == null)
        {
            return Results.NotFound("Star system not found");
        }

        var destinations = await destinationService.GetDestinationsBySystemIdAsync(systemId);
        return Results.Ok(destinations);
    }

    private static async Task<IResult> CreateDestination(DestinationDto destinationDto, IDestinationService destinationService, IStarSystemService starSystemService)
    {
        if (destinationDto.DestinationId != 0)
        {
            return Results.BadRequest("Destination ID should not be provided for creation");
        }

        var starSystem = await starSystemService.GetStarSystemByIdAsync(destinationDto.SystemId);
        if (starSystem == null)
        {
            return Results.BadRequest($"Star system with ID {destinationDto.SystemId} not found");
        }

        var id = await destinationService.CreateDestinationAsync(destinationDto);
        return Results.Created($"/api/destinations/{id}", id);
    }

    private static async Task<IResult> UpdateDestination(int id, DestinationDto destinationDto, IDestinationService destinationService, IStarSystemService starSystemService)
    {
        if (id != destinationDto.DestinationId)
        {
            return Results.BadRequest("ID mismatch");
        }

        var destination = await destinationService.GetDestinationByIdAsync(id);
        if (destination == null)
        {
            return Results.NotFound();
        }

        var starSystem = await starSystemService.GetStarSystemByIdAsync(destinationDto.SystemId);
        if (starSystem == null)
        {
            return Results.BadRequest($"Star system with ID {destinationDto.SystemId} not found");
        }

        await destinationService.UpdateDestinationAsync(destinationDto);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateDestination(int id, IDestinationService destinationService)
    {
        var destination = await destinationService.GetDestinationByIdAsync(id);
        if (destination == null)
        {
            return Results.NotFound();
        }

        await destinationService.DeactivateDestinationAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> ActivateDestination(int id, IDestinationService destinationService)
    {
        var destination = await destinationService.GetDestinationByIdAsync(id);
        if (destination == null)
        {
            return Results.NotFound();
        }

        await destinationService.ActivateDestinationAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllDestinations(
        [FromQuery] string? name,
        [FromQuery] int? systemId,
        [FromQuery] string? systemName,
        [FromQuery] long? minDistance,
        [FromQuery] long? maxDistance,
        [FromQuery] bool? isActive,
        IDestinationService destinationService)
    {
        if (!string.IsNullOrEmpty(name) || systemId.HasValue ||
            !string.IsNullOrEmpty(systemName) || minDistance.HasValue ||
            maxDistance.HasValue || isActive.HasValue)
        {
            var destinations = await destinationService.SearchDestinationsAsync(
                name, systemId, systemName, minDistance, maxDistance, isActive);
            return Results.Ok(destinations);
        }

        var allDestinations = await destinationService.GetAllDestinationsAsync();
        return Results.Ok(allDestinations);
    }
}