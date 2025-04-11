using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class GalaxyService : ApiServiceBase<Galaxy>
{
    public GalaxyService(CommandContext context)
        : base(context, "/api/galaxies")
    {
    }

    public async Task<IEnumerable<Galaxy>> SearchGalaxiesAsync(GalaxySearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(criteria.Name))
                queryParams.Add($"name={Uri.EscapeDataString(criteria.Name)}");

            if (criteria.IsActive.HasValue)
                queryParams.Add($"isActive={criteria.IsActive.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var galaxies = JsonSerializer.Deserialize<List<Galaxy>>(content, JsonOptions);
            return galaxies ?? new List<Galaxy>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching galaxies: {ex.Message}[/]");
            return new List<Galaxy>();
        }
    }
}