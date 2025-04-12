using System.Text;
using System.Text.Json;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services;

public class UserService
{
    protected readonly CommandContext Context;
    protected readonly HttpClient HttpClient;
    protected readonly string BaseUrl;
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserService(CommandContext context)
    {
        Context = context;
        HttpClient = context.HttpClient;
        BaseUrl = "/api/users";
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/me");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserInfo>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching current user: {ex.Message}[/]");
            return null;
        }
    }

    public async Task<IEnumerable<UserInfo>> GetAllUsersAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserInfo>>(content, JsonOptions);
            return users ?? new List<UserInfo>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching users: {ex.Message}[/]");
            return new List<UserInfo>();
        }
    }

    public async Task<UserInfo?> GetUserByIdAsync(string googleId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/{googleId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]User with Google ID {googleId} not found.[/]");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserInfo>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching user: {ex.Message}[/]");
            return null;
        }
    }

    public async Task<IEnumerable<UserInfo>> SearchUsersAsync(UserSearchCriteria criteria)
    {
        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(criteria.Name))
                queryParams.Add($"name={Uri.EscapeDataString(criteria.Name)}");

            if (!string.IsNullOrEmpty(criteria.FirstName))
                queryParams.Add($"firstName={Uri.EscapeDataString(criteria.FirstName)}");

            if (!string.IsNullOrEmpty(criteria.LastName))
                queryParams.Add($"lastName={Uri.EscapeDataString(criteria.LastName)}");

            if (!string.IsNullOrEmpty(criteria.Email))
                queryParams.Add($"email={Uri.EscapeDataString(criteria.Email)}");

            if (!string.IsNullOrEmpty(criteria.Role))
                queryParams.Add($"role={Uri.EscapeDataString(criteria.Role)}");

            if (criteria.IsActive.HasValue)
                queryParams.Add($"isActive={criteria.IsActive.Value}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await HttpClient.GetAsync($"{BaseUrl}{query}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserInfo>>(content, JsonOptions);
            return users ?? new List<UserInfo>();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error searching users: {ex.Message}[/]");
            return new List<UserInfo>();
        }
    }

    public async Task<bool> ActivateUserAsync(string googleId)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{googleId}/activate", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]User with Google ID {googleId} not found.[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error activating user: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(string googleId)
    {
        try
        {
            var response = await HttpClient.PatchAsync($"{BaseUrl}/{googleId}/deactivate", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]User with Google ID {googleId} not found.[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to deactivate user: {content}[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error deactivating user: {ex.Message}[/]");
            return false;
        }
    }

    public async Task<bool> UpdateUserRoleAsync(string googleId, string roleName)
    {
        try
        {
            var updateRoleDto = new UpdateUserRoleDto { RoleName = roleName };
            var content = JsonSerializer.Serialize(updateRoleDto);
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await HttpClient.PatchAsync($"{BaseUrl}/{googleId}/role", stringContent);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[yellow]User with Google ID {googleId} not found.[/]");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[yellow]Failed to update user role: {errorContent}[/]");
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error updating user role: {ex.Message}[/]");
            return false;
        }
    }
}