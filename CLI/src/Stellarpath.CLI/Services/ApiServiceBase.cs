using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;

namespace Stellarpath.CLI.Services;

public abstract class ApiServiceBase<T> where T : class
{
    protected readonly CommandContext Context;
    protected readonly HttpClient HttpClient;
    protected readonly string BaseUrl;
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiServiceBase(CommandContext context, string baseUrl)
    {
        Context = context;
        HttpClient = context.HttpClient;
        BaseUrl = baseUrl;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var entities = JsonSerializer.Deserialize<List<T>>(content, JsonOptions);
            return entities ?? new List<T>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching data: {ex.Message}[/]");
            return new List<T>();
        }
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Item with ID {id} not found.[/]");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching item: {ex.Message}[/]");
            return null;
        }
    }


    public virtual async Task<int?> CreateAsync(T entity)
    {
        try
        {
            var content = JsonSerializer.Serialize(entity);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(BaseUrl, stringContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating item: {ex.Message}[/]");
            return null;
        }
    }

    public virtual async Task<bool> UpdateAsync(T entity, int id)
    {
        try
        {
            var content = JsonSerializer.Serialize(entity);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await HttpClient.PutAsync($"{BaseUrl}/{id}", stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Item with ID {id} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error updating item: {ex.Message}[/]");
            return false;
        }
    }

    public virtual async Task<bool> ActivateAsync(int id)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{id}/activate", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Item with ID {id} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error activating item: {ex.Message}[/]");
            return false;
        }
    }


    public virtual async Task<bool> DeactivateAsync(int id)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{id}/deactivate", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]Item with ID {id} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error deactivating item: {ex.Message}[/]");
            return false;
        }
    }

    public virtual async Task<IEnumerable<T>> GetActiveAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/active");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var entities = JsonSerializer.Deserialize<List<T>>(content, JsonOptions);
            return entities ?? new List<T>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching active items: {ex.Message}[/]");
            return new List<T>();
        }
    }
}