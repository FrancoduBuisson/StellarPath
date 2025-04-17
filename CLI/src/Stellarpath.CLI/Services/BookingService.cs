using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class BookingService : ApiServiceBase<Booking>
{
    public BookingService(CommandContext context)
        : base(context, "/api/bookings")
    {
    }

    public async Task<IEnumerable<Booking>> GetMyBookingsAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var bookings = JsonSerializer.Deserialize<List<Booking>>(content, JsonOptions);
            return bookings ?? new List<Booking>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching your bookings: {ex.Message}[/]");
            return new List<Booking>();
        }
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCruiseAsync(int cruiseId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/cruise/{cruiseId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Cruise with ID {cruiseId} not found.[/]");
                return new List<Booking>();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You are not authorized to view bookings for this cruise.[/]");
                return new List<Booking>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var bookings = JsonSerializer.Deserialize<List<Booking>>(content, JsonOptions);
            return bookings ?? new List<Booking>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching bookings for cruise: {ex.Message}[/]");
            return new List<Booking>();
        }
    }

    public async Task<IEnumerable<int>> GetAvailableSeatsForCruiseAsync(int cruiseId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/cruise/{cruiseId}/seats");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Cruise with ID {cruiseId} not found.[/]");
                return new List<int>();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Error: {error}[/]");
                return new List<int>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var seats = JsonSerializer.Deserialize<List<int>>(content, JsonOptions);
            return seats ?? new List<int>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching available seats: {ex.Message}[/]");
            return new List<int>();
        }
    }
    public async Task<List<int>> CreateMultipleBookingAsync(List<CreateBookingDto> bookingDtos)
    {
        try
        {
            var content = JsonSerializer.Serialize(bookingDtos);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"{BaseUrl}/multi", stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to create bookings: {errorContent}[/]");
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You must be logged in to create a booking.[/]");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            List<int> bookingIdList = new();

            if (!string.IsNullOrWhiteSpace(result))
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<List<int>>(result);
                    if (deserialized != null)
                    {
                        bookingIdList = deserialized;
                    }
                }
                catch (JsonException ex)
                {
                    return null;
                }
            }

            return bookingIdList;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating booking: {ex.Message}[/]");
            return null;
        }
    }



    public async Task<int?> CreateBookingAsync(CreateBookingDto bookingDto)
    {
        try
        {
            var content = JsonSerializer.Serialize(bookingDto);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(BaseUrl, stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to create booking: {errorContent}[/]");
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You must be logged in to create a booking.[/]");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating booking: {ex.Message}[/]");
            return null;
        }
    }

    public async Task<bool> CancelBookingAsync(int id)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{id}/cancel", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Booking with ID {id} not found.[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to cancel booking: {errorContent}[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You are not authorized to cancel this booking.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error cancelling booking: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<bool> PayForBookingAsync(int id)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{id}/pay", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Booking with ID {id} not found.[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to pay for booking: {errorContent}[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You are not authorized to pay for this booking.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error paying for booking: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<IEnumerable<BookingHistory>> SearchBookingHistoryAsync(SearchBookingHistoryDto searchParams)
    {
        try
        {
            var queryParams = new List<string>();

            if (searchParams.BookingId.HasValue)
                queryParams.Add($"bookingId={searchParams.BookingId.Value}");

            if (searchParams.PreviousStatusId.HasValue)
                queryParams.Add($"previousStatusId={searchParams.PreviousStatusId.Value}");

            if (searchParams.NewStatusId.HasValue)
                queryParams.Add($"newStatusId={searchParams.NewStatusId.Value}");

            if (!string.IsNullOrEmpty(searchParams.PreviousStatusName))
                queryParams.Add($"previousStatusName={Uri.EscapeDataString(searchParams.PreviousStatusName)}");

            if (!string.IsNullOrEmpty(searchParams.NewStatusName))
                queryParams.Add($"newStatusName={Uri.EscapeDataString(searchParams.NewStatusName)}");

            if (searchParams.FromDate.HasValue)
                queryParams.Add($"fromDate={Uri.EscapeDataString(searchParams.FromDate.Value.ToString("o"))}");

            if (searchParams.ToDate.HasValue)
                queryParams.Add($"toDate={Uri.EscapeDataString(searchParams.ToDate.Value.ToString("o"))}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}/history/search{query}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You are not authorized to search booking history.[/]");
                return new List<BookingHistory>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var history = JsonSerializer.Deserialize<List<BookingHistory>>(content, JsonOptions);
            return history ?? new List<BookingHistory>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching booking history: {ex.Message}[/]");
            return new List<BookingHistory>();
        }
    }

    public async Task<IEnumerable<Booking>> SearchBookingsAsync(SearchBookingsDto searchParams)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(searchParams.GoogleId))
                queryParams.Add($"googleId={Uri.EscapeDataString(searchParams.GoogleId)}");

            if (searchParams.CruiseId.HasValue)
                queryParams.Add($"cruiseId={searchParams.CruiseId.Value}");

            if (searchParams.BookingStatusId.HasValue)
                queryParams.Add($"bookingStatusId={searchParams.BookingStatusId.Value}");

            if (!string.IsNullOrEmpty(searchParams.StatusName))
                queryParams.Add($"statusName={Uri.EscapeDataString(searchParams.StatusName)}");

            if (searchParams.FromDate.HasValue)
                queryParams.Add($"fromDate={Uri.EscapeDataString(searchParams.FromDate.Value.ToString("o"))}");

            if (searchParams.ToDate.HasValue)
                queryParams.Add($"toDate={Uri.EscapeDataString(searchParams.ToDate.Value.ToString("o"))}");

            if (searchParams.SeatNumber.HasValue)
                queryParams.Add($"seatNumber={searchParams.SeatNumber.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}/search{query}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You are not authorized to search bookings.[/]");
                return new List<Booking>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var bookings = JsonSerializer.Deserialize<List<Booking>>(content, JsonOptions);
            return bookings ?? new List<Booking>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching bookings: {ex.Message}[/]");
            return new List<Booking>();
        }
    }
}