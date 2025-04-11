using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Spectre.Console;
using Stellarpath.CLI.Models;
using StellarPath.CLI.Utility;
using Stellarpath.CLI.Utility;

namespace Stellarpath.CLI.Commands
{
    public class LoginCommandSettings : CommandSettings
    {
        // No specific settings needed for login
    }

    public class LoginCommand : AsyncCommand<LoginCommandSettings>
    {
        private readonly HttpClient _httpClient;
        private readonly ApiClient _apiClient;
        private readonly AppState _appState;

        public LoginCommand(HttpClient httpClient, ApiClient apiClient, AppState appState)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
            _appState = appState;
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] LoginCommandSettings settings)
        {
            if (_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[yellow]You are already logged in. Use 'logout' first if you want to login as a different user.[/]");
                return 0;
            }

            var loginHandler = new LoginHandler(_httpClient);
            var authResult = await loginHandler.LoginAsync();

            if (authResult.Success)
            {
                _appState.JwtToken = authResult.Token;
                _appState.CurrentUser = authResult.User;
                _appState.IsLoggedIn = true;
                _apiClient.SetAuthToken(_appState.JwtToken);

                // Display a welcome message with the user's name
                AnsiConsole.MarkupLine($"[green]Welcome, {_appState.CurrentUser.FirstName} {_appState.CurrentUser.LastName}![/]");
                AnsiConsole.MarkupLine($"[grey]Role: {_appState.CurrentUser.Role}[/]");

                return 0;
            }

            return 1;
        }
    }
}
