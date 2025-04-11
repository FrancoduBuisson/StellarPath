using Spectre.Console;
using Spectre.Console.Cli;
using Stellarpath.CLI.Models;
using StellarPath.CLI.Utility;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace StellarPath.ConsoleClient.Commands
{
    public abstract class CommandSettings
    {

    }

    public abstract class AsyncCommand<T> where T : CommandSettings
    {
        protected readonly ApiClient ApiClient;
        protected readonly AppState AppState;

        protected AsyncCommand(ApiClient apiClient, AppState appState)
        {
            ApiClient = apiClient;
            AppState = appState;
        }

        protected bool RequireAuthentication()
        {
            if (!AppState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[red]You must be logged in to use this command.[/]");
                return false;
            }
            return true;
        }

        protected bool RequireAdminRole()
        {
            if (!RequireAuthentication())
            {
                return false;
            }

            if (AppState.CurrentUser.Role != "Admin")
            {
                AnsiConsole.MarkupLine("[red]This command requires administrator privileges.[/]");
                return false;
            }
            return true;
        }
    }
}