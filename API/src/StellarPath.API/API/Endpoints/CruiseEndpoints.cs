using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;
using System.Security.Claims;

namespace API.Endpoints;
public static class CruiseEndpoints
{
    public static WebApplication RegisterCruiseEndpoints(this WebApplication app)
    {
        var cruiseGroup = app.MapGroup("/api/cruises")
            .WithTags("Cruises");

        cruiseGroup.MapGet("/", GetAllCruises)
            .WithName("GetAllCruises")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/{id}", GetCruiseById)
            .WithName("GetCruiseById")
            .Produces<CruiseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/spaceship/{spaceshipId}", GetCruisesBySpaceshipId)
            .WithName("GetCruisesBySpaceshipId")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/status/{statusId}", GetCruisesByStatus)
            .WithName("GetCruisesByStatus")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/departure/{destinationId}", GetCruisesByDepartureDestination)
            .WithName("GetCruisesByDepartureDestination")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/arrival/{destinationId}", GetCruisesByArrivalDestination)
            .WithName("GetCruisesByArrivalDestination")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/daterange", GetCruisesBetweenDates)
            .WithName("GetCruisesBetweenDates")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        cruiseGroup.MapGet("/myCreated", GetCruisesCreatedByCurrentUser)
            .WithName("GetCruisesCreatedByCurrentUser")
            .Produces<IEnumerable<CruiseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();

        cruiseGroup.MapPost("/", CreateCruise)
            .WithName("CreateCruise")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("Admin");

        cruiseGroup.MapPatch("/{id}/cancel", CancelCruise)
            .WithName("CancelCruise")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("Admin");

        cruiseGroup.MapPatch("/update-statuses", UpdateCruiseStatuses)
            .WithName("UpdateCruiseStatuses")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("Admin");

        return app;
    }

    private static async Task<IResult> GetAllCruises(
        [FromQuery] int? spaceshipId,
        [FromQuery] int? departureDestinationId,
        [FromQuery] int? arrivalDestinationId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? statusId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        ICruiseService cruiseService)
    {
        await cruiseService.UpdateCruiseStatusesAsync();

        if (spaceshipId.HasValue || departureDestinationId.HasValue || arrivalDestinationId.HasValue ||
            startDate.HasValue || endDate.HasValue || statusId.HasValue || minPrice.HasValue || maxPrice.HasValue)
        {
            var cruises = await cruiseService.SearchCruisesAsync(
                spaceshipId, departureDestinationId, arrivalDestinationId,
                startDate, endDate, statusId, minPrice, maxPrice);
            return Results.Ok(cruises);
        }

        var allCruises = await cruiseService.GetAllCruisesAsync();
        return Results.Ok(allCruises);
    }

    private static async Task<IResult> GetCruiseById(int id, ICruiseService cruiseService)
    {
        await cruiseService.UpdateCruiseStatusesAsync();

        var cruise = await cruiseService.GetCruiseByIdAsync(id);
        return cruise != null ? Results.Ok(cruise) : Results.NotFound();
    }

    private static async Task<IResult> GetCruisesBySpaceshipId(int spaceshipId, ICruiseService cruiseService, ISpaceshipService spaceshipService)
    {
        var spaceship = await spaceshipService.GetSpaceshipByIdAsync(spaceshipId);
        if (spaceship == null)
        {
            return Results.NotFound("Spaceship not found");
        }

        var cruises = await cruiseService.GetCruisesBySpaceshipIdAsync(spaceshipId);
        return Results.Ok(cruises);
    }

    private static async Task<IResult> GetCruisesByStatus(int statusId, ICruiseService cruiseService)
    {
        var cruises = await cruiseService.GetCruisesByStatusAsync(statusId);
        return Results.Ok(cruises);
    }

    private static async Task<IResult> GetCruisesByDepartureDestination(int destinationId, ICruiseService cruiseService, IDestinationService destinationService)
    {
        var destination = await destinationService.GetDestinationByIdAsync(destinationId);
        if (destination == null)
        {
            return Results.NotFound("Departure destination not found");
        }

        var cruises = await cruiseService.GetCruisesByDepartureDestinationAsync(destinationId);
        return Results.Ok(cruises);
    }

    private static async Task<IResult> GetCruisesByArrivalDestination(int destinationId, ICruiseService cruiseService, IDestinationService destinationService)
    {
        var destination = await destinationService.GetDestinationByIdAsync(destinationId);
        if (destination == null)
        {
            return Results.NotFound("Arrival destination not found");
        }

        var cruises = await cruiseService.GetCruisesByArrivalDestinationAsync(destinationId);
        return Results.Ok(cruises);
    }

    private static async Task<IResult> GetCruisesBetweenDates(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        ICruiseService cruiseService)
    {
        if (startDate >= endDate)
        {
            return Results.BadRequest("Start date must be before end date");
        }

        var cruises = await cruiseService.GetCruisesBetweenDatesAsync(startDate, endDate);
        return Results.Ok(cruises);
    }

    private static async Task<IResult> GetCruisesCreatedByCurrentUser(ClaimsPrincipal user, ICruiseService cruiseService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        var cruises = await cruiseService.GetCruisesCreatedByUserAsync(googleId);
        return Results.Ok(cruises);
    }

    private static async Task<IResult> CreateCruise(
        [FromBody] CreateCruiseDto cruiseDto,
        ClaimsPrincipal user,
        ICruiseService cruiseService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var id = await cruiseService.CreateCruiseAsync(cruiseDto, googleId);
            return Results.Created($"/api/cruises/{id}", id);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> CancelCruise(int id, ICruiseService cruiseService)
    {
        try
        {
            var success = await cruiseService.CancelCruiseAsync(id);
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> UpdateCruiseStatuses(ICruiseService cruiseService)
    {
        await cruiseService.UpdateCruiseStatusesAsync();
        return Results.NoContent();
    }
}