using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using global::Stellarpath.CLI.Core;
using Spectre.Console;
using Stellarpath.CLI.Models;


namespace Stellarpath.CLI.Services;

public class GalaxyService
{
    private readonly CommandContext _context;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GalaxyService(CommandContext context)
    {
        _context = context;
        _httpClient = context.HttpClient;
    }

    public async Task<IEnumerable<Galaxy>> GetAllGalaxiesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/galaxies");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var galaxies = JsonSerializer.Deserialize<List<Galaxy>>(content, _jsonOptions);
            return galaxies ?? new List<Galaxy>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching galaxies: {ex.Message}[/]");
            return new List<Galaxy>();
        }
    }

    public async Task<IEnumerable<Galaxy>> GetActiveGalaxiesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/galaxies/active");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var galaxies = JsonSerializer.Deserialize<List<Galaxy>>(content, _jsonOptions);
            return galaxies ?? new List<Galaxy>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching active galaxies: {ex.Message}[/]");
            return new List<Galaxy>();
        }
    }

    public async Task<Galaxy?> GetGalaxyByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/galaxies/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Galaxy with ID {id} not found.[/]");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Galaxy>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching galaxy: {ex.Message}[/]");
            return null;
        }
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
            var response = await _httpClient.GetAsync($"/api/galaxies{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var galaxies = JsonSerializer.Deserialize<List<Galaxy>>(content, _jsonOptions);
            return galaxies ?? new List<Galaxy>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching galaxies: {ex.Message}[/]");
            return new List<Galaxy>();
        }
    }

    public async Task<int?> CreateGalaxyAsync(Galaxy galaxy)
    {
        try
        {
            var content = JsonSerializer.Serialize(galaxy);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/galaxies", stringContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating galaxy: {ex.Message}[/]");
            return null;
        }
    }

    public async Task<bool> UpdateGalaxyAsync(Galaxy galaxy)
    {
        try
        {
            var content = JsonSerializer.Serialize(galaxy);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/galaxies/{galaxy.GalaxyId}", stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Galaxy with ID {galaxy.GalaxyId} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error updating galaxy: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<bool> ActivateGalaxyAsync(int id)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"/api/galaxies/{id}/activate", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Galaxy with ID {id} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error activating galaxy: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<bool> DeactivateGalaxyAsync(int id)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"/api/galaxies/{id}/deactivate", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Galaxy with ID {id} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error deactivating galaxy: {ex.Message}[/]");
            return false;
        }
    }
}