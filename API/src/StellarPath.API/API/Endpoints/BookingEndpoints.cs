using Microsoft.AspNetCore.Mvc;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Endpoints
{
    public static class BookingEndpoints
    {
        public static WebApplication RegisterBookingEndpoints(this WebApplication app)
        {
            var bookingsGroup = app.MapGroup("/api/bookings")
                .WithTags("Bookings");
                //.RequireAuthorization(); 

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
                .RequireAuthorization("AdminPolicy"); // Example policy

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
            string googleId)
        {
            try
            {
                var bookings = await bookingService.GetBookingsForUserAsync(googleId);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetUserActiveBookings(
            IBookingService bookingService,
            string googleId)
        {
            try
            {
                var bookings = await bookingService.GetActiveBookingsForUserAsync(googleId);
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
            [FromBody] BookingDto bookingDto)
        {
            try
            {
                var createdBooking = await bookingService.CreateBookingAsync(bookingDto);
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