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

namespace Stellarpath.CLI.Commands
{
    public class LogoutCommandSettings : CommandSettings
    {
        // No specific settings needed for logout
    }

    public class LogoutCommand : Command<LogoutCommandSettings>
    {
        private readonly ApiClient _apiClient;
        private readonly AppState _appState;

        public LogoutCommand(ApiClient apiClient, AppState appState)
        {
            _apiClient = apiClient;
            _appState = appState;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] LogoutCommandSettings settings)
        {
            if (!_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[yellow]You are not currently logged in.[/]");
                return 0;
            }

            // Clear the application state
            _appState.JwtToken = null;
            _appState.CurrentUser = null;
            _appState.IsLoggedIn = false;

            // Clear the API client auth token
            _apiClient.ClearAuthToken();

            // Clear the saved session
            SessionManager.ClearSession();

            AnsiConsole.MarkupLine("[green]Logged out successfully.[/]");
            return 0;
        }
    }
}
