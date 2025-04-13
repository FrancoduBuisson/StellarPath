using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StelarPath.API.Infrastructure.Data.Repositories;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints
{
    public static class BookingEndpoints
    {
        public static WebApplication RegisterBookingEndpoints(this WebApplication app)
        {
            var bookingsGroup = app.MapGroup("/api/bookings")
                .WithTags("Bookings")
                .RequireAuthorization();

            bookingsGroup.MapGet("/", GetAllBookings)
                .WithName("GetAllBookings")
                .Produces<IEnumerable<BookingDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapGet("/{id:int}", GetBookingById)
                .WithName("GetBookingById")
                .Produces<BookingDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapGet("/user/{googleId}", GetUserBookings)
                .WithName("GetUserBookings")
                .Produces<IEnumerable<BookingDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapGet("/user/{googleId}/active", GetUserActiveBookings)
                .WithName("GetUserActiveBookings")
                .Produces<IEnumerable<BookingDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapGet("/cruise/{cruiseId:int}", GetCruiseBookings)
                .WithName("GetCruiseBookings")
                .Produces<IEnumerable<BookingDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapGet("/{bookingId:int}/history", GetBookingHistory)
                .WithName("GetBookingHistory")
                .Produces<IEnumerable<BookingHistoryDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapPost("/", CreateBooking)
                .WithName("CreateBooking")
                .Produces<BookingDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapPut("/{id:int}", UpdateBooking)
                .WithName("UpdateBooking")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapPut("/{id:int}/cancel", CancelBooking)
                .WithName("CancelBooking")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            bookingsGroup.MapPut("/{id:int}/confirm", ConfirmBooking)
                .WithName("ConfirmBooking")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            var adminBookingsGroup = app.MapGroup("/api/admin/bookings")
                .WithTags("Bookings Admin")
                .RequireAuthorization("Admin");

            adminBookingsGroup.MapPost("/expire", ExpireOldBookings)
                .WithName("ExpireOldBookings")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status500InternalServerError);

            return app;
        }

        private static async Task<IResult> GetAllBookings(
            IBookingService bookingService,
            [FromQuery] int? cruiseId,
            [FromQuery] string? status)
        {
            try
            {
                IEnumerable<BookingDto> bookings;
                if (cruiseId.HasValue)
                {
                    bookings = await bookingService.GetBookingsForCruiseAsync(cruiseId.Value);
                }

                else
                {
                    bookings = await bookingService.GetAllBookingsAsync();
                }
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetBookingById(
            IBookingService bookingService,
            int id)
        {
            try
            {
                var booking = await bookingService.GetBookingByIdAsync(id);
                return booking is not null ? Results.Ok(booking) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetUserBookings(
            IBookingService bookingService,
            IUserService userService,
            IUserProvider userProvider,
            [FromQuery] string? googleId = "")
        {
            try
            {
                string userId = googleId;

                if (string.IsNullOrEmpty(userId)) {
                    userId = userProvider.GetCurrentUserId();
                }
                
                var user = await userService.GetUserByGoogleIdAsync(userId);
                var bookings = await bookingService.GetBookingsForUserAsync(user.GoogleId);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetUserActiveBookings(
            IBookingService bookingService,
            IUserProvider userProvider,
            [FromQuery] string? googleId = "")
        {
            try
            {
                var userId = googleId;

                if (string.IsNullOrEmpty(userId))
                {
                    userId = userProvider.GetCurrentUserId();
                }

                var bookings = await bookingService.GetActiveBookingsForUserAsync(userId);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetCruiseBookings(
            IBookingService bookingService,
            int cruiseId)
        {
            try
            {
                var bookings = await bookingService.GetBookingsForCruiseAsync(cruiseId);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetBookingHistory(
            IBookingService bookingService,
            int bookingId)
        {
            try
            {
                var history = await bookingService.GetBookingHistoryAsync(bookingId);
                return Results.Ok(history);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> CreateBooking(
            IBookingService bookingService,
            ICruiseService cruiseService,
            ICruiseStatusService cruiseStatusService,
            ISpaceshipService spaceshipService,
            IShipModelService shipModelService,
            int seatNumber,
            int cruiseId)
        {
            try
            {
                var cruise = await cruiseService.GetCruiseByIdAsync(cruiseId);

                var scheduleCruiseStatus = await cruiseStatusService.GetScheduledStatusIdAsync();

                if (cruise == null)
                {
                    return Results.NotFound("Cruise not found");
                }

                if (cruise.CruiseStatusId != scheduleCruiseStatus)
                {
                    return Results.BadRequest("This cruise is not available for booking");
                }

                if (cruise.SpaceshipName == null)
                {
                    return Results.NotFound("No spaceship found for cruise");
                }

                var totalBookings = await bookingService.GetBookedSeatsCountForCruiseAsync(cruise.CruiseId);

                var availableSeats = await cruiseService.GetAvailableSeatsForCruiseAsync(cruise.CruiseId);

                if (seatNumber < 0 || seatNumber > cruise.Capacity)
                {
                    return Results.BadRequest(new
                    {
                        Title = "Invalid seat selection",
                        Detail = "The requested seat is is invalid",
                        AvailableSeats = availableSeats,
                    });
                }

                bool isSeatAvailable = await bookingService.IsSeatAvailableForCruiseAsync(cruiseId, seatNumber);

                if (!isSeatAvailable)
                {
                    return Results.Conflict(new
                    {
                        Title = "Invalid seat selection",
                        Detail = "The requested seat is is invalid",
                        AvailableSeats = availableSeats,
                    });
                }

                var createdBooking = await bookingService.CreateBookingAsync(cruiseId, seatNumber);
                return Results.Created($"/api/bookings/{createdBooking.BookingId}", createdBooking);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> UpdateBooking(
            IBookingService bookingService,
            int id,
            [FromBody] BookingDto bookingDto)
        {
            try
            {
                if (id != bookingDto.BookingId)
                    return Results.BadRequest("ID mismatch");

                var success = await bookingService.UpdateBookingAsync(bookingDto);
                return success ? Results.NoContent() : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> CancelBooking(
            IBookingService bookingService,
            int id)
        {
            try
            {
                var success = await bookingService.CancelBookingAsync(id);
                return success ? Results.NoContent() : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> ConfirmBooking(
            IBookingService bookingService,
            int id)
        {
            try
            {
                var success = await bookingService.ConfirmBookingAsync(id);
                return success ? Results.NoContent() : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> ExpireOldBookings(
            IBookingService bookingService,
            [FromQuery] DateTime? cutoffDate)
        {
            try
            {
                var actualCutoff = cutoffDate ?? DateTime.UtcNow.AddDays(-1);
                await bookingService.ExpireOldBookingsAsync(actualCutoff);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}