using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class SpaceshipService : ApiServiceBase<Spaceship>
{
    public SpaceshipService(CommandContext context)
        : base(context, "/api/spaceships")
    {
    }
    public async Task<IEnumerable<Spaceship>> SearchSpaceshipsAsync(SpaceshipSearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (criteria.ModelId.HasValue)
                queryParams.Add($"modelId={criteria.ModelId.Value}");

            if (!string.IsNullOrEmpty(criteria.ModelName))
                queryParams.Add($"modelName={Uri.EscapeDataString(criteria.ModelName)}");

            if (criteria.IsActive.HasValue)
                queryParams.Add($"isActive={criteria.IsActive.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var spaceships = JsonSerializer.Deserialize<List<Spaceship>>(content, JsonOptions);
            return spaceships ?? new List<Spaceship>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching spaceships: {ex.Message}[/]");
            return new List<Spaceship>();
        }
    }

    public async Task<IEnumerable<SpaceshipAvailability>> GetAvailableSpaceshipsForTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var query = $"?startTime={Uri.EscapeDataString(startTime.ToString("o"))}&endTime={Uri.EscapeDataString(endTime.ToString("o"))}";
            var response = await HttpClient.GetAsync($"{BaseUrl}/available{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var availableSpaceships = JsonSerializer.Deserialize<List<SpaceshipAvailability>>(content, JsonOptions);
            return availableSpaceships ?? new List<SpaceshipAvailability>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching available spaceships: {ex.Message}[/]");
            return new List<SpaceshipAvailability>();
        }
    }

    public async Task<(bool success, int cancelledCruises)> DeactivateAsync(int id, bool cancelCruises = false)
    {
        try
        {
            var query = cancelCruises ? $"?cancelCruises={cancelCruises}" : "";
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{id}/deactivate{query}", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Item with ID {id} not found.[/]");
                return (false, 0);
            }

            response.EnsureSuccessStatusCode();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DeactivationResult>(content, JsonOptions);
                return (true, result?.CancelledCruises ?? 0);
            }

            return (true, 0);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error deactivating item: {ex.Message}[/]");
            return (false, 0);
        }
    }
}