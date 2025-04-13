using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class DestinationCommandHandler : CommandHandlerBase<Destination>
{
    private readonly DestinationService _destinationService;
    private readonly StarSystemService _starSystemService;

    public DestinationCommandHandler(CommandContext context, DestinationService destinationService, StarSystemService starSystemService)
        : base(context)
    {
        _destinationService = destinationService;
        _starSystemService = starSystemService;
    }

    protected override string GetMenuTitle() => "Destination Management";
    protected override string GetEntityName() => "Destination";
    protected override string GetEntityNamePlural() => "Destinations";

    protected override List<string> GetEntitySpecificOptions()
    {
        return new List<string>
        {
            "View Destinations by Star System"
        };
    }

    protected override async Task ProcessSelectionAsync(string selection)
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
        await ExecuteWithSpinnerAsync("Fetching destinations...", async ctx =>
        {
            var destinations = await _destinationService.GetAllAsync();
            DisplayEntities(destinations, "All Destinations");
            return true;
        });
    }

    private async Task ListActiveDestinationsAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching active destinations...", async ctx =>
        {
            var destinations = await _destinationService.GetActiveAsync();
            DisplayEntities(destinations, "Active Destinations");
            return true;
        });
    }

    private async Task ViewDestinationDetailsAsync()
    {
        var destination = await FetchAndPromptForEntitySelectionAsync<DestinationService, Destination>(
            _destinationService,
            service => service.GetAllAsync(),
            d => d.Name,
            d => d.DestinationId,
            "Fetching destinations for selection...",
            "No destinations found.",
            "Select a destination to view details");

        if (destination != null)
        {
            await ExecuteWithSpinnerAsync($"Fetching details for destination ID {destination.DestinationId}...", async ctx =>
            {
                var fetchedDestination = await _destinationService.GetByIdAsync(destination.DestinationId);
                if (fetchedDestination != null)
                {
                    DisplayEntityDetails(fetchedDestination);
                }
                return true;
            });
        }
    }

    private async Task SearchDestinationsAsync()
    {
        var searchCriteria = new DestinationSearchCriteria();

        InputHelper.CollectSearchCriteria<DestinationSearchCriteria>(
            "name",
            criteria => criteria.Name = InputHelper.AskForString("Enter destination name (or part of name):"),
            searchCriteria);

        var includeSystem = InputHelper.AskForConfirmation("Do you want to filter by star system?", false);
        if (includeSystem)
        {
            var bySystemId = InputHelper.AskForConfirmation("Search by system ID? (No = search by system name)", false);
            if (bySystemId)
            {
                searchCriteria.SystemId = InputHelper.AskForInt("Enter star system ID:");
            }
            else
            {
                searchCriteria.SystemName = InputHelper.AskForString("Enter star system name (or part of name):");
            }
        }

        var includeDistance = InputHelper.AskForConfirmation("Do you want to filter by distance from Earth?", false);
        if (includeDistance)
        {
            var includeMinDistance = InputHelper.AskForConfirmation("Set minimum distance?", false);
            if (includeMinDistance)
            {
                searchCriteria.MinDistance = InputHelper.AskForLong("Enter minimum distance from Earth (km):");
            }

            var includeMaxDistance = InputHelper.AskForConfirmation("Set maximum distance?", false);
            if (includeMaxDistance)
            {
                searchCriteria.MaxDistance = InputHelper.AskForLong("Enter maximum distance from Earth (km):");
            }
        }

        InputHelper.CollectSearchCriteria<DestinationSearchCriteria>(
            "active status",
            criteria => criteria.IsActive = InputHelper.AskForConfirmation("Show only active destinations?"),
            searchCriteria);

        await ExecuteWithSpinnerAsync("Searching destinations...", async ctx =>
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

            DisplayEntities(destinations, title);
            return true;
        });
    }

    private async Task ViewDestinationsByStarSystemAsync()
    {
        var starSystem = await FetchAndPromptForEntitySelectionAsync<StarSystemService, StarSystem>(
            _starSystemService,
            service => service.GetAllAsync(),
            s => s.SystemName,
            s => s.SystemId,
            "Fetching star systems for selection...",
            "No star systems found.",
            "Select a star system to view its destinations");

        if (starSystem == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching destinations for star system ID {starSystem.SystemId}...", async ctx =>
        {
            var destinations = await _destinationService.GetDestinationsBySystemIdAsync(starSystem.SystemId);
            DisplayEntities(destinations, $"Destinations in Star System: {starSystem.SystemName}");
            return true;
        });
    }

    private async Task CreateDestinationAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var starSystems = await ExecuteWithSpinnerAsync(
            "Fetching active star systems for selection...",
            async ctx => await _starSystemService.GetActiveAsync());

        if (!starSystems.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active star systems found. Please create a star system first.[/]");
            return;
        }

        var selectedStarSystem = SelectionHelper.SelectFromListById(
            starSystems,
            s => s.SystemId,
            s => s.SystemName,
            "Select the star system for this destination");

        if (selectedStarSystem == null)
        {
            return;
        }

        var newDestination = new Destination
        {
            Name = InputHelper.AskForString("Enter destination name:"),
            SystemId = selectedStarSystem.SystemId,
            DistanceFromEarth = InputHelper.AskForLong("Enter distance from Earth (in kilometers):"),
            IsActive = InputHelper.AskForConfirmation("Should this destination be active?", true)
        };

        await ExecuteWithSpinnerAsync("Creating destination...", async ctx =>
        {
            var result = await _destinationService.CreateAsync(newDestination);

            if (result.HasValue)
            {
                AnsiConsole.MarkupLine($"[green]Destination created successfully with ID: {result.Value}[/]");

                var destination = await _destinationService.GetByIdAsync(result.Value);
                if (destination != null)
                {
                    DisplayEntityDetails(destination);
                }
            }
            return true;
        });
    }

    private async Task UpdateDestinationAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var destination = await FetchAndPromptForEntitySelectionAsync<DestinationService, Destination>(
            _destinationService,
            service => service.GetAllAsync(),
            d => d.Name,
            d => d.DestinationId,
            "Fetching destinations for selection...",
            "No destinations found to update.",
            "Select a destination to update");

        if (destination == null)
        {
            return;
        }

        var starSystems = await ExecuteWithSpinnerAsync(
            "Fetching star systems...",
            async ctx => await _starSystemService.GetAllAsync());

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

        destination.Name = InputHelper.AskForString("Enter new name:", destination.Name);

        var changeSystem = InputHelper.AskForConfirmation("Do you want to change the star system?", false);
        if (changeSystem)
        {
            var selectedStarSystem = SelectionHelper.SelectFromListById(
                starSystems,
                s => s.SystemId,
                s => s.SystemName,
                "Select the new star system for this destination");

            if (selectedStarSystem != null)
            {
                destination.SystemId = selectedStarSystem.SystemId;
            }
        }

        destination.DistanceFromEarth = InputHelper.AskForLong("Enter new distance from Earth (in kilometers):", destination.DistanceFromEarth);
        destination.IsActive = InputHelper.AskForConfirmation("Should this destination be active?", destination.IsActive);

        await ExecuteWithSpinnerAsync("Updating destination...", async ctx =>
        {
            var result = await _destinationService.UpdateAsync(destination, destination.DestinationId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Destination updated successfully![/]");

                var updatedDestination = await _destinationService.GetByIdAsync(destination.DestinationId);
                if (updatedDestination != null)
                {
                    DisplayEntityDetails(updatedDestination);
                }
            }
            return true;
        });
    }

    private async Task ActivateDestinationAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var allDestinations = await ExecuteWithSpinnerAsync(
            "Fetching inactive destinations...",
            async ctx => await _destinationService.GetAllAsync());

        var inactiveDestinations = allDestinations.Where(d => !d.IsActive).ToList();

        if (!inactiveDestinations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No inactive destinations found.[/]");
            return;
        }

        var selectedDestination = SelectionHelper.SelectFromListById(
            inactiveDestinations,
            d => d.DestinationId,
            d => d.Name,
            "Select a destination to activate");

        if (selectedDestination == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Activating destination ID {selectedDestination.DestinationId}...", async ctx =>
        {
            try {
                var result = await _destinationService.ActivateAsync(selectedDestination.DestinationId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Destination activated successfully![/]");

                    var destination = await _destinationService.GetByIdAsync(selectedDestination.DestinationId);
                    if (destination != null)
                    {
                        DisplayEntityDetails(destination);
                    }
                }
                return true;
            }catch (HttpRequestException ex) when (ex.Message.Contains("400"))
            {
                AnsiConsole.MarkupLine("[red]Cannot activate this destination.[/]");
                AnsiConsole.MarkupLine("[yellow]The parent star system or galaxy is inactive. Please activate the star system first.[/]");
                return false;
            }
            
        });
    }

    private async Task DeactivateDestinationAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var activeDestinations = await ExecuteWithSpinnerAsync(
            "Fetching active destinations...",
            async ctx => await _destinationService.GetActiveAsync());

        if (!activeDestinations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active destinations found.[/]");
            return;
        }

        var selectedDestination = SelectionHelper.SelectFromListById(
            activeDestinations,
            d => d.DestinationId,
            d => d.Name,
            "Select a destination to deactivate");

        if (selectedDestination == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Deactivating destination ID {selectedDestination.DestinationId}...", async ctx =>
        {
            var result = await _destinationService.DeactivateAsync(selectedDestination.DestinationId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Destination deactivated successfully![/]");

                var destination = await _destinationService.GetByIdAsync(selectedDestination.DestinationId);
                if (destination != null)
                {
                    DisplayEntityDetails(destination);
                }
            }
            return true;
        });
    }

    protected override void DisplayEntities(IEnumerable<Destination> destinations, string title)
    {
        var destinationList = destinations.ToList();

        var columns = new[] { "ID", "Name", "Star System", "Distance from Earth", "Status" };

        var rows = destinationList.Select(d => new[]
        {
            d.DestinationId.ToString(),
            d.Name,
            $"{d.SystemName ?? "Unknown"} (ID: {d.SystemId})",
            $"{d.DistanceFromEarth:N0} km",
            DisplayHelper.FormatActiveStatus(d.IsActive)
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(Destination destination)
    {
        var details = new Dictionary<string, string>
        {
            ["Destination ID"] = destination.DestinationId.ToString(),
            ["Name"] = destination.Name,
            ["Star System"] = $"{destination.SystemName ?? "Unknown"} (ID: {destination.SystemId})",
            ["Distance from Earth"] = $"{destination.DistanceFromEarth:N0} km",
            ["Status"] = DisplayHelper.FormatActiveStatus(destination.IsActive)
        };

        DisplayHelper.DisplayDetails("Destination Details", details);
    }
}