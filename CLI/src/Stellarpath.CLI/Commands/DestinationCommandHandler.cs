using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class DestinationCommandHandler
{
    private readonly CommandContext _context;
    private readonly DestinationService _destinationService;
    private readonly StarSystemService _starSystemService;

    public DestinationCommandHandler(CommandContext context, DestinationService destinationService, StarSystemService starSystemService)
    {
        _context = context;
        _destinationService = destinationService;
        _starSystemService = starSystemService;
    }

    public async Task HandleAsync()
    {
        var options = new List<string>
        {
            "List All Destinations",
            "List Active Destinations",
            "View Destination Details",
            "Search Destinations",
            "View Destinations by Star System"
        };

        if (_context.CurrentUser?.Role == "Admin")
        {
            options.AddRange(new[]
            {
                "Create New Destination",
                "Update Destination",
                "Activate Destination",
                "Deactivate Destination"
            });
        }

        options.Add("Back to Main Menu");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Destination Management")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        await ProcessSelectionAsync(selection);
    }

    private async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "List All Destinations":
                await ListAllDestinationsAsync();
                break;
            case "List Active Destinations":
                await ListActiveDestinationsAsync();
                break;
            case "View Destination Details":
                await ViewDestinationDetailsAsync();
                break;
            case "Search Destinations":
                await SearchDestinationsAsync();
                break;
            case "View Destinations by Star System":
                await ViewDestinationsByStarSystemAsync();
                break;
            case "Create New Destination":
                await CreateDestinationAsync();
                break;
            case "Update Destination":
                await UpdateDestinationAsync();
                break;
            case "Activate Destination":
                await ActivateDestinationAsync();
                break;
            case "Deactivate Destination":
                await DeactivateDestinationAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ListAllDestinationsAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching destinations...", async ctx =>
            {
                var destinations = await _destinationService.GetAllAsync();
                DisplayDestinations(destinations, "All Destinations");
            });
    }

    private async Task ListActiveDestinationsAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active destinations...", async ctx =>
            {
                var destinations = await _destinationService.GetActiveAsync();
                DisplayDestinations(destinations, "Active Destinations");
            });
    }

    private async Task ViewDestinationDetailsAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching destinations for selection...", async ctx =>
            {
                var destinations = await _destinationService.GetAllAsync();

                if (!destinations.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No destinations found.[/]");
                    return;
                }

                ctx.Status("Select a destination to view details");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allDestinations = await _destinationService.GetAllAsync();
        if (!allDestinations.Any())
        {
            return;
        }

        var destinationNames = allDestinations.Select(d => $"{d.DestinationId}: {d.Name}").ToList();
        var selectedDestinationName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a destination to view details")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(destinationNames));

        int destinationId = int.Parse(selectedDestinationName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching details for destination ID {destinationId}...", async ctx =>
            {
                var destination = await _destinationService.GetByIdAsync(destinationId);

                if (destination != null)
                {
                    DisplayDestinationDetails(destination);
                }
            });
    }

    private async Task SearchDestinationsAsync()
    {
        var searchCriteria = new DestinationSearchCriteria();

        var includeName = AnsiConsole.Confirm("Do you want to search by name?", false);
        if (includeName)
        {
            searchCriteria.Name = AnsiConsole.Ask<string>("Enter destination name (or part of name):");
        }

        var includeSystem = AnsiConsole.Confirm("Do you want to filter by star system?", false);
        if (includeSystem)
        {
            var bySystemId = AnsiConsole.Confirm("Search by system ID? (No = search by system name)", false);
            if (bySystemId)
            {
                searchCriteria.SystemId = AnsiConsole.Ask<int>("Enter star system ID:");
            }
            else
            {
                searchCriteria.SystemName = AnsiConsole.Ask<string>("Enter star system name (or part of name):");
            }
        }

        var includeDistance = AnsiConsole.Confirm("Do you want to filter by distance from Earth?", false);
        if (includeDistance)
        {
            var includeMinDistance = AnsiConsole.Confirm("Set minimum distance?", false);
            if (includeMinDistance)
            {
                searchCriteria.MinDistance = AnsiConsole.Ask<long>("Enter minimum distance from Earth (km):");
            }

            var includeMaxDistance = AnsiConsole.Confirm("Set maximum distance?", false);
            if (includeMaxDistance)
            {
                searchCriteria.MaxDistance = AnsiConsole.Ask<long>("Enter maximum distance from Earth (km):");
            }
        }

        var includeStatus = AnsiConsole.Confirm("Do you want to filter by active status?", false);
        if (includeStatus)
        {
            searchCriteria.IsActive = AnsiConsole.Confirm("Show only active destinations?");
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Searching destinations...", async ctx =>
            {
                var destinations = await _destinationService.SearchDestinationsAsync(searchCriteria);

                string title = "Search Results for Destinations";
                if (!string.IsNullOrEmpty(searchCriteria.Name))
                {
                    title += $" containing '{searchCriteria.Name}'";
                }
                if (searchCriteria.SystemId.HasValue)
                {
                    title += $" in star system ID {searchCriteria.SystemId.Value}";
                }
                if (!string.IsNullOrEmpty(searchCriteria.SystemName))
                {
                    title += $" in star system '{searchCriteria.SystemName}'";
                }
                if (searchCriteria.MinDistance.HasValue)
                {
                    title += $" min distance {searchCriteria.MinDistance.Value} km";
                }
                if (searchCriteria.MaxDistance.HasValue)
                {
                    title += $" max distance {searchCriteria.MaxDistance.Value} km";
                }
                if (searchCriteria.IsActive.HasValue)
                {
                    title += $" (Status: {(searchCriteria.IsActive.Value ? "Active" : "Inactive")})";
                }

                DisplayDestinations(destinations, title);
            });
    }

    private async Task ViewDestinationsByStarSystemAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching star systems for selection...", async ctx =>
            {
                var starSystems = await _starSystemService.GetAllAsync();

                if (!starSystems.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No star systems found.[/]");
                    return;
                }

                ctx.Status("Select a star system to view its destinations");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var starSystems = await _starSystemService.GetAllAsync();
        if (!starSystems.Any())
        {
            return;
        }

        var systemNames = starSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
        var selectedSystemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a star system to view its destinations")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(systemNames));

        int systemId = int.Parse(selectedSystemName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching destinations for star system ID {systemId}...", async ctx =>
            {
                var destinations = await _destinationService.GetDestinationsBySystemIdAsync(systemId);

                var systemName = starSystems.FirstOrDefault(s => s.SystemId == systemId)?.SystemName ?? $"ID: {systemId}";
                DisplayDestinations(destinations, $"Destinations in Star System: {systemName}");
            });
    }

    private async Task CreateDestinationAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to create destinations.[/]");
            return;
        }

        // First, get star systems for selection
        var starSystems = await _starSystemService.GetActiveAsync();
        if (!starSystems.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active star systems found. Please create a star system first.[/]");
            return;
        }

        var systemNames = starSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
        var selectedSystemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select the star system for this destination")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(systemNames));

        int systemId = int.Parse(selectedSystemName.Split(':')[0].Trim());

        var newDestination = new Destination
        {
            Name = AnsiConsole.Ask<string>("Enter destination name:"),
            SystemId = systemId,
            DistanceFromEarth = AnsiConsole.Ask<long>("Enter distance from Earth (in kilometers):"),
            IsActive = AnsiConsole.Confirm("Should this destination be active?", true)
        };

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Creating destination...", async ctx =>
            {
                var result = await _destinationService.CreateAsync(newDestination);

                if (result.HasValue)
                {
                    AnsiConsole.MarkupLine($"[green]Destination created successfully with ID: {result.Value}[/]");

                    var destination = await _destinationService.GetByIdAsync(result.Value);
                    if (destination != null)
                    {
                        DisplayDestinationDetails(destination);
                    }
                }
            });
    }

    private async Task UpdateDestinationAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to update destinations.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching destinations for selection...", async ctx =>
            {
                var destinations = await _destinationService.GetAllAsync();

                if (!destinations.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No destinations found to update.[/]");
                    return;
                }

                ctx.Status("Select a destination to update");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allDestinations = await _destinationService.GetAllAsync();
        if (!allDestinations.Any())
        {
            return;
        }

        var destinationNames = allDestinations.Select(d => $"{d.DestinationId}: {d.Name}").ToList();
        var selectedDestinationName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a destination to update")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(destinationNames));

        int destinationId = int.Parse(selectedDestinationName.Split(':')[0].Trim());

        var destination = await _destinationService.GetByIdAsync(destinationId);
        if (destination == null)
        {
            return;
        }

        var starSystems = await _starSystemService.GetAllAsync();
        if (!starSystems.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No star systems found. Cannot update star system reference.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Destination ID: {destination.DestinationId}[/]");
        AnsiConsole.MarkupLine($"Current Name: [yellow]{destination.Name}[/]");
        AnsiConsole.MarkupLine($"Current Star System: [yellow]{destination.SystemName} (ID: {destination.SystemId})[/]");
        AnsiConsole.MarkupLine($"Current Distance from Earth: [yellow]{destination.DistanceFromEarth} km[/]");
        AnsiConsole.MarkupLine($"Current Status: [yellow]{(destination.IsActive ? "Active" : "Inactive")}[/]");

        destination.Name = AnsiConsole.Ask("Enter new name:", destination.Name);

        var changeSystem = AnsiConsole.Confirm("Do you want to change the star system?", false);
        if (changeSystem)
        {
            var systemNames = starSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
            var selectedSystemName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the new star system for this destination")
                    .PageSize(10)
                    .HighlightStyle(new Style(Color.Green))
                    .AddChoices(systemNames));

            destination.SystemId = int.Parse(selectedSystemName.Split(':')[0].Trim());
        }

        destination.DistanceFromEarth = AnsiConsole.Ask("Enter new distance from Earth (in kilometers):", destination.DistanceFromEarth);
        destination.IsActive = AnsiConsole.Confirm("Should this destination be active?", destination.IsActive);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Updating destination...", async ctx =>
            {
                var result = await _destinationService.UpdateAsync(destination, destination.DestinationId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Destination updated successfully![/]");

                    var updatedDestination = await _destinationService.GetByIdAsync(destination.DestinationId);
                    if (updatedDestination != null)
                    {
                        DisplayDestinationDetails(updatedDestination);
                    }
                }
            });
    }

    private async Task ActivateDestinationAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to activate destinations.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching inactive destinations...", async ctx =>
            {
                var allDestinations = await _destinationService.GetAllAsync();
                var inactiveDestinations = allDestinations.Where(d => !d.IsActive).ToList();

                if (!inactiveDestinations.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No inactive destinations found.[/]");
                    return;
                }

                ctx.Status("Select a destination to activate");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allDestinations = await _destinationService.GetAllAsync();
        var inactiveDestinations = allDestinations.Where(d => !d.IsActive).ToList();

        if (!inactiveDestinations.Any())
        {
            return;
        }

        var destinationNames = inactiveDestinations.Select(d => $"{d.DestinationId}: {d.Name}").ToList();
        var selectedDestinationName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a destination to activate")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(destinationNames));

        int destinationId = int.Parse(selectedDestinationName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Activating destination ID {destinationId}...", async ctx =>
            {
                var result = await _destinationService.ActivateAsync(destinationId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Destination activated successfully![/]");

                    var destination = await _destinationService.GetByIdAsync(destinationId);
                    if (destination != null)
                    {
                        DisplayDestinationDetails(destination);
                    }
                }
            });
    }

    private async Task DeactivateDestinationAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to deactivate destinations.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active destinations...", async ctx =>
            {
                var activeDestinations = await _destinationService.GetActiveAsync();

                if (!activeDestinations.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No active destinations found.[/]");
                    return;
                }

                ctx.Status("Select a destination to deactivate");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var activeDestinations = await _destinationService.GetActiveAsync();

        if (!activeDestinations.Any())
        {
            return;
        }

        var destinationNames = activeDestinations.Select(d => $"{d.DestinationId}: {d.Name}").ToList();
        var selectedDestinationName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a destination to deactivate")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(destinationNames));

        int destinationId = int.Parse(selectedDestinationName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Deactivating destination ID {destinationId}...", async ctx =>
            {
                var result = await _destinationService.DeactivateAsync(destinationId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Destination deactivated successfully![/]");

                    var destination = await _destinationService.GetByIdAsync(destinationId);
                    if (destination != null)
                    {
                        DisplayDestinationDetails(destination);
                    }
                }
            });
    }

    private void DisplayDestinations(IEnumerable<Destination> destinations, string title)
    {
        var destinationList = destinations.ToList();

        var columns = new[] { "ID", "Name", "Star System", "Distance from Earth", "Status" };

        var rows = destinationList.Select(d => new[]
        {
            d.DestinationId.ToString(),
            d.Name,
            $"{d.SystemName} (ID: {d.SystemId})",
            $"{d.DistanceFromEarth:N0} km",
            DisplayHelper.FormatActiveStatus(d.IsActive)
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    private void DisplayDestinationDetails(Destination destination)
    {
        var details = new Dictionary<string, string>
        {
            ["Destination ID"] = destination.DestinationId.ToString(),
            ["Name"] = destination.Name,
            ["Star System"] = $"{destination.SystemName} (ID: {destination.SystemId})",
            ["Distance from Earth"] = $"{destination.DistanceFromEarth:N0} km",
            ["Status"] = DisplayHelper.FormatActiveStatus(destination.IsActive)
        };

        DisplayHelper.DisplayDetails("Destination Details", details);
    }
}