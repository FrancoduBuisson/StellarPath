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
    public class GalaxyDto
    {
        public int GalaxyId { get; set; }
        public string GalaxyName { get; set; }
        public bool IsActive { get; set; }
    }

    public class StarSystemDto
    {
        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public int GalaxyId { get; set; }
        public string GalaxyName { get; set; }
        public bool IsActive { get; set; }
    }

    public class DestinationDto
    {
        public int DestinationId { get; set; }
        public string Name { get; set; }
        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public long DistanceFromEarth { get; set; }
        public bool IsActive { get; set; }
    }

    // List Galaxies Command
    public class ListGalaxiesCommandSettings : CommandSettings
    {
        [CommandOption("-a|--active")]
        [Description("Show only active galaxies")]
        public bool ActiveOnly { get; set; }
    }

    public class ListGalaxiesCommand : AsyncCommand<ListGalaxiesCommandSettings>
    {
        private readonly ApiClient _apiClient;
        private readonly AppState _appState;

        public ListGalaxiesCommand(ApiClient apiClient, AppState appState)
        {
            _apiClient = apiClient;
            _appState = appState;
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ListGalaxiesCommandSettings settings)
        {
            if (!_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[red]You must be logged in to use this command.[/]");
                return 1;
            }

            try
            {
                List<GalaxyDto> galaxies;

                if (settings.ActiveOnly)
                {
                    galaxies = await _apiClient.GetAsync<List<GalaxyDto>>("/api/galaxies/active");
                }
                else
                {
                    galaxies = await _apiClient.GetAsync<List<GalaxyDto>>("/api/galaxies");
                }

                if (galaxies.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No galaxies found.[/]");
                    return 0;
                }

                var table = new Table();
                table.AddColumn("ID");
                table.AddColumn("Galaxy Name");
                table.AddColumn("Status");

                foreach (var galaxy in galaxies)
                {
                    table.AddRow(
                        galaxy.GalaxyId.ToString(),
                        galaxy.GalaxyName,
                        galaxy.IsActive ? "[green]Active[/]" : "[red]Inactive[/]"
                    );
                }

                AnsiConsole.Write(table);
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error retrieving galaxies: {ex.Message}[/]");
                return 1;
            }
        }
    }

    // List Star Systems Command
    public class ListStarSystemsCommandSettings : CommandSettings
    {
        [CommandOption("-a|--active")]
        [Description("Show only active star systems")]
        public bool ActiveOnly { get; set; }

        [CommandOption("-g|--galaxy")]
        [Description("Filter by galaxy ID")]
        public int? GalaxyId { get; set; }
    }

    public class ListStarSystemsCommand : AsyncCommand<ListStarSystemsCommandSettings>
    {
        private readonly ApiClient _apiClient;
        private readonly AppState _appState;

        public ListStarSystemsCommand(ApiClient apiClient, AppState appState)
        {
            _apiClient = apiClient;
            _appState = appState;
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ListStarSystemsCommandSettings settings)
        {
            if (!_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[red]You must be logged in to use this command.[/]");
                return 1;
            }

            try
            {
                List<StarSystemDto> starSystems;

                if (settings.ActiveOnly)
                {
                    starSystems = await _apiClient.GetAsync<List<StarSystemDto>>("/api/starsystems/active");
                }
                else if (settings.GalaxyId.HasValue)
                {
                    starSystems = await _apiClient.GetAsync<List<StarSystemDto>>($"/api/starsystems/galaxy/{settings.GalaxyId.Value}");
                }
                else
                {
                    starSystems = await _apiClient.GetAsync<List<StarSystemDto>>("/api/starsystems");
                }

                if (starSystems.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No star systems found.[/]");
                    return 0;
                }

                var table = new Table();
                table.AddColumn("ID");
                table.AddColumn("System Name");
                table.AddColumn("Galaxy");
                table.AddColumn("Status");

                foreach (var system in starSystems)
                {
                    table.AddRow(
                        system.SystemId.ToString(),
                        system.SystemName,
                        system.GalaxyName ?? "Unknown",
                        system.IsActive ? "[green]Active[/]" : "[red]Inactive[/]"
                    );
                }

                AnsiConsole.Write(table);
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error retrieving star systems: {ex.Message}[/]");
                return 1;
            }
        }
    }

    // List Destinations Command
    public class ListDestinationsCommandSettings : CommandSettings
    {
        [CommandOption("-a|--active")]
        [Description("Show only active destinations")]
        public bool ActiveOnly { get; set; }

        [CommandOption("-s|--system")]
        [Description("Filter by star system ID")]
        public int? SystemId { get; set; }
    }

    public class ListDestinationsCommand : AsyncCommand<ListDestinationsCommandSettings>
    {
        private readonly ApiClient _apiClient;
        private readonly AppState _appState;

        public ListDestinationsCommand(ApiClient apiClient, AppState appState)
        {
            _apiClient = apiClient;
            _appState = appState;
        }

        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ListDestinationsCommandSettings settings)
        {
            if (!_appState.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("[red]You must be logged in to use this command.[/]");
                return 1;
            }

            try
            {
                List<DestinationDto> destinations;

                if (settings.ActiveOnly)
                {
                    destinations = await _apiClient.GetAsync<List<DestinationDto>>("/api/destinations/active");
                }
                else if (settings.SystemId.HasValue)
                {
                    destinations = await _apiClient.GetAsync<List<DestinationDto>>($"/api/destinations/system/{settings.SystemId.Value}");
                }
                else
                {
                    destinations = await _apiClient.GetAsync<List<DestinationDto>>("/api/destinations");
                }

                if (destinations.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No destinations found.[/]");
                    return 0;
                }

                var table = new Table();
                table.AddColumn("ID");
                table.AddColumn("Name");
                table.AddColumn("System");
                table.AddColumn("Distance from Earth");
                table.AddColumn("Status");

                foreach (var destination in destinations)
                {
                    table.AddRow(
                        destination.DestinationId.ToString(),
                        destination.Name,
                        destination.SystemName ?? "Unknown",
                        FormatDistance(destination.DistanceFromEarth),
                        destination.IsActive ? "[green]Active[/]" : "[red]Inactive[/]"
                    );
                }

                AnsiConsole.Write(table);
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error retrieving destinations: {ex.Message}[/]");
                return 1;
            }
        }

        private string FormatDistance(long distanceKm)
        {
            if (distanceKm < 1000)
            {
                return $"{distanceKm} km";
            }
            else if (distanceKm < 1_000_000)
            {
                return $"{distanceKm / 1000.0:N1} thousand km";
            }
            else if (distanceKm < 1_000_000_000)
            {
                return $"{distanceKm / 1_000_000.0:N1} million km";
            }
            else
            {
                return $"{distanceKm / 1_000_000_000.0:N1} billion km";
            }
        }
    }
}
