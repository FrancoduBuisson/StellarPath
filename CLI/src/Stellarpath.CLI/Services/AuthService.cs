using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.UI;
using StellarPath.ConsoleClient.Services.Auth;
using System.Text.Json;

namespace Stellarpath.CLI.Services;

public class AuthService
{
  private readonly CommandContext _context;
  private readonly LoginHandler _loginHandler;

  public AuthService(CommandContext context)
  {
    _context = context;
    _loginHandler = new LoginHandler(context.HttpClient);
  }

  public async Task LoginAsync()
  {
    NasaApodResponse? apod = null;

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Star)
        .SpinnerStyle("green")
        .StartAsync("Starting authentication process...", async ctx =>
        {
          var (idToken, accessToken) = await _loginHandler.GetGoogleAuth();
          if (string.IsNullOrEmpty(idToken))
          {
            AnsiConsole.MarkupLine("[red]Failed to authenticate with Google.[/]");
            return;
          }

          var result = await _loginHandler.AuthenticateWithBackend((idToken, accessToken));
          if (!result.Success)
          {
            AnsiConsole.MarkupLine("[red]Failed to authenticate with backend.[/]");
            return;
          }

          _context.SetAuth(result.Token, result.User);
          AnsiConsole.MarkupLine($"[green]Welcome, {result.User.FirstName}![/]");

          AnsiConsole.MarkupLine("[grey]Fetching NASA Picture of the Day...[/]");

          try
          {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/nasa/apod");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            var response = await _context.HttpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
              var content = await response.Content.ReadAsStringAsync();
              var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
              apod = JsonSerializer.Deserialize<NasaApodResponse>(content, options);
            }
            else
            {
              AnsiConsole.MarkupLine($"[red]Failed to fetch APOD: {response.StatusCode}[/]");
            }
          }
          catch (Exception ex)
          {
            AnsiConsole.MarkupLine($"[red]Error fetching APOD: {ex.Message}[/]");
          }
        });

    if (apod != null)
    {
      ApodDisplayHelper.ShowApod(apod);
    }
    else
    {
      AnsiConsole.MarkupLine("[yellow]Could not retrieve APOD.[/]");
    }

    HelpRenderer.ShowHelp();
  }

  public void Logout()
  {
    _context.ClearAuth();
    AnsiConsole.MarkupLine("[yellow]Logged out successfully.[/]");
  }
}

