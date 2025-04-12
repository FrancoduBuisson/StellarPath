using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class CruiseService : ApiServiceBase<Cruise>
{
    public CruiseService(CommandContext context)
        : base(context, "/api/cruises")
    {
    }

    public async Task<IEnumerable<Cruise>> SearchCruisesAsync(CruiseSearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (criteria.SpaceshipId.HasValue)
                queryParams.Add($"spaceshipId={criteria.SpaceshipId.Value}");

            if (criteria.DepartureDestinationId.HasValue)
                queryParams.Add($"departureDestinationId={criteria.DepartureDestinationId.Value}");

            if (criteria.ArrivalDestinationId.HasValue)
                queryParams.Add($"arrivalDestinationId={criteria.ArrivalDestinationId.Value}");

            if (criteria.StartDate.HasValue)
                queryParams.Add($"startDate={Uri.EscapeDataString(criteria.StartDate.Value.ToString("o"))}");

            if (criteria.EndDate.HasValue)
                queryParams.Add($"endDate={Uri.EscapeDataString(criteria.EndDate.Value.ToString("o"))}");

            if (criteria.StatusId.HasValue)
                queryParams.Add($"statusId={criteria.StatusId.Value}");

            if (criteria.MinPrice.HasValue)
                queryParams.Add($"minPrice={criteria.MinPrice.Value}");

            if (criteria.MaxPrice.HasValue)
                queryParams.Add($"maxPrice={criteria.MaxPrice.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching cruises: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<IEnumerable<Cruise>> GetCruisesBySpaceshipIdAsync(int spaceshipId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/spaceship/{spaceshipId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Spaceship with ID {spaceshipId} not found.[/]");
                return new List<Cruise>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching cruises by spaceship ID: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<IEnumerable<Cruise>> GetCruisesByDepartureDestinationAsync(int destinationId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/departure/{destinationId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Destination with ID {destinationId} not found.[/]");
                return new List<Cruise>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching cruises by departure destination ID: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<IEnumerable<Cruise>> GetCruisesByArrivalDestinationAsync(int destinationId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/arrival/{destinationId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Destination with ID {destinationId} not found.[/]");
                return new List<Cruise>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching cruises by arrival destination ID: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<IEnumerable<Cruise>> GetCruisesBetweenDatesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var query = $"?startDate={Uri.EscapeDataString(startDate.ToString("o"))}&endDate={Uri.EscapeDataString(endDate.ToString("o"))}";
            var response = await HttpClient.GetAsync($"{BaseUrl}/daterange{query}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Invalid date range: {errorContent}[/]");
                return new List<Cruise>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching cruises between dates: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<IEnumerable<Cruise>> GetCruisesByStatusIdAsync(int statusId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/status/{statusId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching cruises by status ID: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<IEnumerable<Cruise>> GetMyCruisesAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/myCreated");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You must be logged in to view your cruises.[/]");
                return new List<Cruise>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var cruises = JsonSerializer.Deserialize<List<Cruise>>(content, JsonOptions);
            return cruises ?? new List<Cruise>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching your cruises: {ex.Message}[/]");
            return new List<Cruise>();
        }
    }

    public async Task<int?> CreateCruiseAsync(CreateCruiseDto cruiseDto)
    {
        try
        {
            var content = JsonSerializer.Serialize(cruiseDto);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(BaseUrl, stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to create cruise: {errorContent}[/]");
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You must be logged in with admin privileges to create cruises.[/]");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating cruise: {ex.Message}[/]");
            return null;
        }
    }

    public async Task<bool> CancelCruiseAsync(int id)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{id}/cancel", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Cruise with ID {id} not found.[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to cancel cruise: {errorContent}[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You must be logged in with admin privileges to cancel cruises.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error cancelling cruise: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<bool> UpdateCruiseStatusesAsync()
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/update-statuses", null);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AnsiConsole.MarkupLine("[yellow]You must be logged in with admin privileges to update cruise statuses.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error updating cruise statuses: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<IEnumerable<CruiseStatus>> GetAllCruiseStatusesAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync("/api/cruisestatuses");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<CruiseStatus>
                {
                    new CruiseStatus { CruiseStatusId = 1, StatusName = "Scheduled" },
                    new CruiseStatus { CruiseStatusId = 2, StatusName = "In Progress" },
                    new CruiseStatus { CruiseStatusId = 3, StatusName = "Completed" },
                    new CruiseStatus { CruiseStatusId = 4, StatusName = "Cancelled" }
                };
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var statuses = JsonSerializer.Deserialize<List<CruiseStatus>>(content, JsonOptions);
            return statuses ?? new List<CruiseStatus>();
        }
        catch (Exception)
        {
            return new List<CruiseStatus>
            {
                new CruiseStatus { CruiseStatusId = 1, StatusName = "Scheduled" },
                new CruiseStatus { CruiseStatusId = 2, StatusName = "In Progress" },
                new CruiseStatus { CruiseStatusId = 3, StatusName = "Completed" },
                new CruiseStatus { CruiseStatusId = 4, StatusName = "Cancelled" }
            };
        }
    }
}