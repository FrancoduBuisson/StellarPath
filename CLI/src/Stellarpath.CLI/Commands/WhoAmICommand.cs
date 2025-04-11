using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Spectre.Console;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Commands
{
    public class WhoAmICommandSettings : CommandSettings
    {
        // No specific settings needed for whoami
    }

    public class WhoAmICommand : Command<WhoAmICommandSettings>
    {
        private readonly AppState _appState;

        public WhoAmICommand(AppState appState)
        {
            _appState = appState;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] WhoAmICommandSettings settings)
        {
            if (!_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[red]You are not currently logged in.[/]");
                return 1;
            }

            var table = new Table();
            table.AddColumn("Field");
            table.AddColumn("Value");

            table.AddRow("Name", $"{_appState.CurrentUser.FirstName} {_appState.CurrentUser.LastName}");
            table.AddRow("Email", _appState.CurrentUser.Email);
            table.AddRow("Google ID", _appState.CurrentUser.GoogleId);
            table.AddRow("Role", _appState.CurrentUser.Role);
            table.AddRow("Active", _appState.CurrentUser.IsActive ? "Yes" : "No");

            AnsiConsole.Write(table);
            return 0;
        }
    }
}
