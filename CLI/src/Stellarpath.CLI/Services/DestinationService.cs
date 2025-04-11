using System.Text;
using System.Text.Json; 
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class DestinationService : ApiServiceBase<Destination>
{
    public DestinationService(CommandContext context)
        : base(context, "/api/destinations")
    {
    }

    public async Task<IEnumerable<Destination>> SearchDestinationsAsync(DestinationSearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(criteria.Name))
                queryParams.Add($"name={Uri.EscapeDataString(criteria.Name)}");

            if (criteria.SystemId.HasValue)
                queryParams.Add($"systemId={criteria.SystemId.Value}");

            if (!string.IsNullOrEmpty(criteria.SystemName))
                queryParams.Add($"systemName={Uri.EscapeDataString(criteria.SystemName)}");

            if (criteria.MinDistance.HasValue)
                queryParams.Add($"minDistance={criteria.MinDistance.Value}");

            if (criteria.MaxDistance.HasValue)
                queryParams.Add($"maxDistance={criteria.MaxDistance.Value}");

            if (criteria.IsActive.HasValue)
                queryParams.Add($"isActive={criteria.IsActive.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var destinations = JsonSerializer.Deserialize<List<Destination>>(content, JsonOptions);
            return destinations ?? new List<Destination>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching destinations: {ex.Message}[/]");
            return new List<Destination>();
        }
    }

    public async Task<IEnumerable<Destination>> GetDestinationsBySystemIdAsync(int systemId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/system/{systemId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Star system with ID {systemId} not found.[/]");
                return new List<Destination>();
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var destinations = JsonSerializer.Deserialize<List<Destination>>(content, JsonOptions);
            return destinations ?? new List<Destination>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching destinations by system ID: {ex.Message}[/]");
            return new List<Destination>();
        }
    }
}