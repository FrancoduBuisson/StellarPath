using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class SpaceshipDto
    {
        public int SpaceshipId { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public int? Capacity { get; set; }
        public int? CruiseSpeedKmph { get; set; }
        public bool IsActive { get; set; }
    }
    public class SpaceshipDetailCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<id>")]
        [Description("The ID of the spaceship to view")]
        public int SpaceshipId { get; set; }
    }

    public class SpaceshipDetailCommand : AsyncCommand<SpaceshipDetailCommandSettings>
    {
        private readonly ApiClient _apiClient;
        private readonly AppState _appState;

        public SpaceshipDetailCommand(ApiClient apiClient, AppState appState)
        {
            _apiClient = apiClient;
            _appState = appState;
        }

        public class ListSpaceshipsCommandSettings : CommandSettings
        {
            [CommandOption("-a|--active")]
            [Description("Show only active spaceships")]
            public bool ActiveOnly { get; set; }

            [CommandOption("-m|--model")]
            [Description("Filter by model ID")]
            public int? ModelId { get; set; }
        }

        public class ListSpaceshipsCommand : AsyncCommand<ListSpaceshipsCommandSettings>
        {
            private readonly ApiClient _apiClient;
            private readonly AppState _appState;

            public ListSpaceshipsCommand(ApiClient apiClient, AppState appState)
            {
                _apiClient = apiClient;
                _appState = appState;
            }

            public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ListSpaceshipsCommandSettings settings)
            {
                if (!_appState.IsLoggedIn)
                {
                    AnsiConsole.MarkupLine("[red]You must be logged in to use this command.[/]");
                    return 1;
                }

                try
                {
                    List<SpaceshipDto> spaceships;

                    if (settings.ActiveOnly)
                    {
                        spaceships = await _apiClient.GetAsync<List<SpaceshipDto>>("/api/spaceships/active");
                    }
                    else if (settings.ModelId.HasValue)
                    {
                        spaceships = await _apiClient.GetAsync<List<SpaceshipDto>>($"/api/spaceships/model/{settings.ModelId.Value}");
                    }
                    else
                    {
                        spaceships = await _apiClient.GetAsync<List<SpaceshipDto>>("/api/spaceships");
                    }

                    if (spaceships.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[yellow]No spaceships found.[/]");
                        return 0;
                    }

                    var table = new Table();
                    table.AddColumn("ID");
                    table.AddColumn("Model");
                    table.AddColumn("Capacity");
                    table.AddColumn("Speed (km/h)");
                    table.AddColumn("Status");

                    foreach (var ship in spaceships)
                    {
                        table.AddRow(
                            ship.SpaceshipId.ToString(),
                            ship.ModelName ?? "Unknown",
                            ship.Capacity?.ToString() ?? "Unknown",
                            ship.CruiseSpeedKmph?.ToString() ?? "Unknown",
                            ship.IsActive ? "[green]Active[/]" : "[red]Inactive[/]"
                        );
                    }

                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error retrieving spaceships: {ex.Message}[/]");
                    return 1;
                }
            }
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] SpaceshipDetailCommandSettings settings)
        {
            if (!_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[red]You must be logged in to use this command.[/]");
                return 1;
            }

            try
            {
                var spaceship = await _apiClient.GetAsync<SpaceshipDto>($"/api/spaceships/{settings.SpaceshipId}");

                var panel = new Panel(new Grid()
                    .AddColumn()
                    .AddColumn()
                    .AddRow("ID:", spaceship.SpaceshipId.ToString())
                    .AddRow("Model:", spaceship.ModelName ?? "Unknown")
                    .AddRow("Model ID:", spaceship.ModelId.ToString())
                    .AddRow("Capacity:", spaceship.Capacity?.ToString() ?? "Unknown")
                    .AddRow("Cruise Speed:", spaceship.CruiseSpeedKmph?.ToString() ?? "Unknown" + " km/h")
                    .AddRow("Status:", spaceship.IsActive ? "[green]Active[/]" : "[red]Inactive[/]"));

                panel.Header = new PanelHeader($"Spaceship {spaceship.SpaceshipId}");
                panel.Border = BoxBorder.Rounded;

                AnsiConsole.Write(panel);
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error retrieving spaceship details: {ex.Message}[/]");
                return 1;
            }
        }
    }
}
