using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class StarSystemService : ApiServiceBase<StarSystem>
{
    public StarSystemService(CommandContext context)
        : base(context, "/api/starsystems")
    {
    }

    public async Task<IEnumerable<StarSystem>> SearchStarSystemsAsync(StarSystemSearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(criteria.Name))
                queryParams.Add($"name={Uri.EscapeDataString(criteria.Name)}");

            if (criteria.GalaxyId.HasValue)
                queryParams.Add($"galaxyId={criteria.GalaxyId.Value}");

            if (!string.IsNullOrEmpty(criteria.GalaxyName))
                queryParams.Add($"galaxyName={Uri.EscapeDataString(criteria.GalaxyName)}");

            if (criteria.IsActive.HasValue)
                queryParams.Add($"isActive={criteria.IsActive.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var starSystems = JsonSerializer.Deserialize<List<StarSystem>>(content, JsonOptions);
            return starSystems ?? new List<StarSystem>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching star systems: {ex.Message}[/]");
            return new List<StarSystem>();
        }
    }

    public async Task<IEnumerable<StarSystem>> GetStarSystemsByGalaxyIdAsync(int galaxyId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/galaxy/{galaxyId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Galaxy with ID {galaxyId} not found.[/]");
                return new List<StarSystem>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var starSystems = JsonSerializer.Deserialize<List<StarSystem>>(content, JsonOptions);
            return starSystems ?? new List<StarSystem>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching star systems by galaxy ID: {ex.Message}[/]");
            return new List<StarSystem>();
        }
    }
}