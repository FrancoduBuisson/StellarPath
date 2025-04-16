using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class PlanetService : ApiServiceBase<PlanetInformation>
{
    public PlanetService(CommandContext context)
        : base(context, "/api/planets/details")
    {
    }

    public async Task<IEnumerable<PlanetInformation>> GetPlanetDetailsByNameAsync(string name)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}?name={Uri.EscapeDataString(name)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]No planet information found for '{name}'.[/]");
                return new List<PlanetInformation>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var planets = JsonSerializer.Deserialize<List<PlanetInformation>>(content, JsonOptions);
            return planets ?? new List<PlanetInformation>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching planet details: {ex.Message}[/]");
            return new List<PlanetInformation>();
        }
    }
}