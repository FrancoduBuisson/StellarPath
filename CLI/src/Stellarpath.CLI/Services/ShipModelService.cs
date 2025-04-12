using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class ShipModelService : ApiServiceBase<ShipModel>
{
    public ShipModelService(CommandContext context)
        : base(context, "/api/shipmodels")
    {
    }

    public async Task<IEnumerable<ShipModel>> SearchShipModelsAsync(ShipModelSearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(criteria.Name))
                queryParams.Add($"name={Uri.EscapeDataString(criteria.Name)}");

            if (criteria.MinCapacity.HasValue)
                queryParams.Add($"minCapacity={criteria.MinCapacity.Value}");

            if (criteria.MaxCapacity.HasValue)
                queryParams.Add($"maxCapacity={criteria.MaxCapacity.Value}");

            if (criteria.MinSpeed.HasValue)
                queryParams.Add($"minSpeed={criteria.MinSpeed.Value}");

            if (criteria.MaxSpeed.HasValue)
                queryParams.Add($"maxSpeed={criteria.MaxSpeed.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var shipModels = JsonSerializer.Deserialize<List<ShipModel>>(content, JsonOptions);
            return shipModels ?? new List<ShipModel>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching ship models: {ex.Message}[/]");
            return new List<ShipModel>();
        }
    }
}