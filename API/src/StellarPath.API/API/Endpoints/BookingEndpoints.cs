using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;
using System.Security.Claims;

namespace API.Endpoints;

public static class BookingEndpoints
{
    public static WebApplication RegisterBookingEndpoints(this WebApplication app)
    {
        var bookingGroup = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        bookingGroup.MapGet("/", GetMyBookings)
            .WithName("GetMyBookings")
            .Produces<IEnumerable<BookingDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();

        bookingGroup.MapGet("/{id}", GetBookingById)
            .WithName("GetBookingById")
            .Produces<BookingDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();

        bookingGroup.MapGet("/cruise/{cruiseId}", GetBookingsByCruise)
            .WithName("GetBookingsByCruise")
            .Produces<IEnumerable<BookingDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("Admin");

        bookingGroup.MapGet("/cruise/{cruiseId}/seats", GetAvailableSeats)
            .WithName("GetAvailableSeats")
            .Produces<IEnumerable<int>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        bookingGroup.MapPost("/", CreateBooking)
            .WithName("CreateBooking")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();

        bookingGroup.MapPatch("/{id}/cancel", CancelBooking)
            .WithName("CancelBooking")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();

        bookingGroup.MapPatch("/{id}/pay", PayForBooking)
            .WithName("PayForBooking")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetMyBookings(
        ClaimsPrincipal user,
        IBookingService bookingService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        var bookings = await bookingService.GetBookingsByUserAsync(googleId);
        return Results.Ok(bookings);
    }

    private static async Task<IResult> GetBookingById(
        int id,
        ClaimsPrincipal user,
        IBookingService bookingService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        var booking = await bookingService.GetBookingByIdAsync(id);

        if (booking == null)
        {
            return Results.NotFound();
        }

        if (booking.GoogleId != googleId && !user.IsInRole("Admin"))
        {
            return Results.Unauthorized();
        }

        return Results.Ok(booking);
    }

    private static async Task<IResult> GetBookingsByCruise(
        int cruiseId,
        IBookingService bookingService,
        ICruiseService cruiseService)
    {
        var cruise = await cruiseService.GetCruiseByIdAsync(cruiseId);
        if (cruise == null)
        {
            return Results.NotFound("Cruise not found");
        }

        var bookings = await bookingService.GetBookingsByCruiseAsync(cruiseId);
        return Results.Ok(bookings);
    }

    private static async Task<IResult> GetAvailableSeats(
        int cruiseId,
        IBookingService bookingService,
        ICruiseService cruiseService)
    {
        try
        {
            var cruise = await cruiseService.GetCruiseByIdAsync(cruiseId);
            if (cruise == null)
            {
                return Results.NotFound("Cruise not found");
            }

            var availableSeats = await bookingService.GetAvailableSeatsForCruiseAsync(cruiseId);
            return Results.Ok(availableSeats);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> CreateBooking(
        [FromBody] CreateBookingDto bookingDto,
        ClaimsPrincipal user,
        IBookingService bookingService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var bookingId = await bookingService.CreateBookingAsync(bookingDto, googleId);
            return Results.Created($"/api/bookings/{bookingId}", bookingId);
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

    private static async Task<IResult> CancelBooking(
        int id,
        ClaimsPrincipal user,
        IBookingService bookingService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await bookingService.CancelBookingAsync(id, googleId);
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> PayForBooking(
        int id,
        ClaimsPrincipal user,
        IBookingService bookingService)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(googleId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var success = await bookingService.PayForBookingAsync(id, googleId);
            if (!success)
            {
                return Results.NotFound();
            }
            return Results.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}