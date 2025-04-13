using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class SpaceshipCommandHandler : CommandHandlerBase<Spaceship>
{
    private readonly SpaceshipService _spaceshipService;
    private readonly ShipModelService _shipModelService;

    public SpaceshipCommandHandler(CommandContext context, SpaceshipService spaceshipService, ShipModelService shipModelService)
        : base(context)
    {
        _spaceshipService = spaceshipService;
        _shipModelService = shipModelService;
    }

    protected override string GetMenuTitle() => "Spaceship Management";
    protected override string GetEntityName() => "Spaceship";
    protected override string GetEntityNamePlural() => "Spaceships";


    protected override List<string> GetEntitySpecificOptions()
    {
        return new List<string>
        {
            "View Spaceships by Model",
            "Check Spaceship Availability"
        };
    }

    protected override async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "List All Spaceships":
                await ListAllSpaceshipsAsync();
                break;
            case "List Active Spaceships":
                await ListActiveSpaceshipsAsync();
                break;
            case "View Spaceship Details":
                await ViewSpaceshipDetailsAsync();
                break;
            case "Search Spaceships":
                await SearchSpaceshipsAsync();
                break;
            case "View Spaceships by Model":
                await ViewSpaceshipsByModelAsync();
                break;
            case "Check Spaceship Availability":
                await CheckSpaceshipAvailabilityAsync();
                break;
            case "Create New Spaceship":
                await CreateSpaceshipAsync();
                break;
            case "Update Spaceship":
                await UpdateSpaceshipAsync();
                break;
            case "Activate Spaceship":
                await ActivateSpaceshipAsync();
                break;
            case "Deactivate Spaceship":
                await DeactivateSpaceshipAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ListAllSpaceshipsAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching spaceships...", async ctx =>
        {
            var spaceships = await _spaceshipService.GetAllAsync();
            DisplayEntities(spaceships, "All Spaceships");
            return true;
        });
    }

    private async Task ListActiveSpaceshipsAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching active spaceships...", async ctx =>
        {
            var spaceships = await _spaceshipService.GetActiveAsync();
            DisplayEntities(spaceships, "Active Spaceships");
            return true;
        });
    }

    private async Task ViewSpaceshipDetailsAsync()
    {
        var spaceship = await FetchAndPromptForEntitySelectionAsync<SpaceshipService, Spaceship>(
            _spaceshipService,
            service => service.GetAllAsync(),
            s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
            s => s.SpaceshipId,
            "Fetching spaceships for selection...",
            "No spaceships found.",
            "Select a spaceship to view details");

        if (spaceship != null)
        {
            await ExecuteWithSpinnerAsync($"Fetching details for spaceship ID {spaceship.SpaceshipId}...", async ctx =>
            {
                var fetchedSpaceship = await _spaceshipService.GetByIdAsync(spaceship.SpaceshipId);
                if (fetchedSpaceship != null)
                {
                    DisplayEntityDetails(fetchedSpaceship);
                }
                return true;
            });
        }
    }

private async Task SearchSpaceshipsAsync()
{
    var searchCriteria = new SpaceshipSearchCriteria();

    var criteriaOptions = new List<string>
    {
        "Ship Model Filter",
        "Active Status"
    };

    var selectedCriteria = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
            .Title("[yellow]Select search criteria to include[/]")
            .PageSize(10)
            .InstructionsText("[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
            .AddChoices(criteriaOptions));

    if (selectedCriteria.Contains("Ship Model Filter"))
    {
        var modelFilterOptions = new List<string>
        {
            "Search by Model ID",
            "Search by Model Name"
        };

        var modelFilterChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]How would you like to filter by ship model?[/]")
                .PageSize(10)
                .AddChoices(modelFilterOptions));

        if (modelFilterChoice == "Search by Model ID")
        {
            searchCriteria.ModelId = InputHelper.AskForInt("[cyan]Enter ship model ID:[/]");
        }
        else
        {
            searchCriteria.ModelName = InputHelper.AskForString("[cyan]Enter ship model name (or part of name):[/]");
        }
    }

    if (selectedCriteria.Contains("Active Status"))
    {
        var statusOptions = new List<string> { "All Spaceships", "Active Only", "Inactive Only" };
        var statusSelection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Select active status filter[/]")
                .PageSize(10)
                .AddChoices(statusOptions));

        switch (statusSelection)
        {
            case "Active Only":
                searchCriteria.IsActive = true;
                break;
            case "Inactive Only":
                searchCriteria.IsActive = false;
                break;
            case "All Spaceships":
            default:
                searchCriteria.IsActive = null;
                break;
        }
    }

    await ExecuteWithSpinnerAsync("Searching spaceships...", async ctx =>
    {
        var spaceships = await _spaceshipService.SearchSpaceshipsAsync(searchCriteria);

        string title = "Search Results for Spaceships";
        if (searchCriteria.ModelId.HasValue)
        {
            title += $" with model ID {searchCriteria.ModelId.Value}";
        }
        if (!string.IsNullOrEmpty(searchCriteria.ModelName))
        {
            title += $" with model name containing '{searchCriteria.ModelName}'";
        }
        if (searchCriteria.IsActive.HasValue)
        {
            title += $" (Status: {(searchCriteria.IsActive.Value ? "Active" : "Inactive")})";
        }

        DisplayEntities(spaceships, title);
        return true;
    });
}
    private async Task ViewSpaceshipsByModelAsync()
    {
        var shipModel = await FetchAndPromptForEntitySelectionAsync<ShipModelService, ShipModel>(
            _shipModelService,
            service => service.GetAllAsync(),
            m => m.ModelName,
            m => m.ModelId,
            "Fetching ship models for selection...",
            "No ship models found.",
            "Select a ship model to view its spaceships");

        if (shipModel == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching spaceships for model ID {shipModel.ModelId}...", async ctx =>
        {
            var criteria = new SpaceshipSearchCriteria { ModelId = shipModel.ModelId };
            var spaceships = await _spaceshipService.SearchSpaceshipsAsync(criteria);

            DisplayEntities(spaceships, $"Spaceships with Model: {shipModel.ModelName}");
            return true;
        });
    }

    private async Task CheckSpaceshipAvailabilityAsync()
    {
        var startDate = InputHelper.AskForDateTime("Enter start date and time (YYYY-MM-DD HH:MM):", DateTime.Now);
        var endDate = InputHelper.AskForDateTime("Enter end date and time (YYYY-MM-DD HH:MM):", startDate.AddDays(1));

        if (startDate >= endDate)
        {
            AnsiConsole.MarkupLine("[yellow]Start time must be before end time. Please try again.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync($"Checking spaceship availability from {DisplayHelper.FormatDateTime(startDate)} to {DisplayHelper.FormatDateTime(endDate)}...", async ctx =>
        {
            var availableSpaceships = await _spaceshipService.GetAvailableSpaceshipsForTimeRangeAsync(startDate, endDate);

            if (!availableSpaceships.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No spaceships are available for the selected time range.[/]");
                return true;
            }

            DisplayAvailableSpaceships(availableSpaceships, $"Available Spaceships from {DisplayHelper.FormatDateTime(startDate)} to {DisplayHelper.FormatDateTime(endDate)}");
            return true;
        });
    }

    private async Task CreateSpaceshipAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var shipModels = await ExecuteWithSpinnerAsync(
            "Fetching ship models for selection...",
            async ctx => await _shipModelService.GetAllAsync());

        if (!shipModels.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No ship models found. Please create a ship model first.[/]");
            return;
        }

        var selectedShipModel = SelectionHelper.SelectFromListById(
            shipModels,
            m => m.ModelId,
            m => m.ModelName,
            "Select the ship model for this spaceship");

        if (selectedShipModel == null)
        {
            return;
        }

        var newSpaceship = new Spaceship
        {
            ModelId = selectedShipModel.ModelId,
            IsActive = InputHelper.AskForConfirmation("Should this spaceship be active?", true)
        };

        await ExecuteWithSpinnerAsync("Creating spaceship...", async ctx =>
        {
            var result = await _spaceshipService.CreateAsync(newSpaceship);

            if (result.HasValue)
            {
                AnsiConsole.MarkupLine($"[green]Spaceship created successfully with ID: {result.Value}[/]");

                var spaceship = await _spaceshipService.GetByIdAsync(result.Value);
                if (spaceship != null)
                {
                    DisplayEntityDetails(spaceship);
                }
            }
            return true;
        });
    }

    private async Task UpdateSpaceshipAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var spaceship = await FetchAndPromptForEntitySelectionAsync<SpaceshipService, Spaceship>(
            _spaceshipService,
            service => service.GetAllAsync(),
            s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
            s => s.SpaceshipId,
            "Fetching spaceships for selection...",
            "No spaceships found to update.",
            "Select a spaceship to update");

        if (spaceship == null)
        {
            return;
        }

        var shipModels = await ExecuteWithSpinnerAsync(
            "Fetching ship models...",
            async ctx => await _shipModelService.GetAllAsync());

        if (!shipModels.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No ship models found. Cannot update ship model reference.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Spaceship ID: {spaceship.SpaceshipId}[/]");
        AnsiConsole.MarkupLine($"Current Model: [yellow]{spaceship.ModelName} (ID: {spaceship.ModelId})[/]");
        AnsiConsole.MarkupLine($"Current Status: [yellow]{(spaceship.IsActive ? "Active" : "Inactive")}[/]");

        var changeModel = InputHelper.AskForConfirmation("Do you want to change the ship model?", false);
        if (changeModel)
        {
            var selectedShipModel = SelectionHelper.SelectFromListById(
                shipModels,
                m => m.ModelId,
                m => m.ModelName,
                "Select the new ship model for this spaceship");

            if (selectedShipModel != null)
            {
                spaceship.ModelId = selectedShipModel.ModelId;
            }
        }

        spaceship.IsActive = InputHelper.AskForConfirmation("Should this spaceship be active?", spaceship.IsActive);

        await ExecuteWithSpinnerAsync("Updating spaceship...", async ctx =>
        {
            var result = await _spaceshipService.UpdateAsync(spaceship, spaceship.SpaceshipId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Spaceship updated successfully![/]");

                var updatedSpaceship = await _spaceshipService.GetByIdAsync(spaceship.SpaceshipId);
                if (updatedSpaceship != null)
                {
                    DisplayEntityDetails(updatedSpaceship);
                }
            }
            return true;
        });
    }

    private async Task ActivateSpaceshipAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var allSpaceships = await ExecuteWithSpinnerAsync(
            "Fetching inactive spaceships...",
            async ctx => await _spaceshipService.GetAllAsync());

        var inactiveSpaceships = allSpaceships.Where(s => !s.IsActive).ToList();

        if (!inactiveSpaceships.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No inactive spaceships found.[/]");
            return;
        }

        var selectedSpaceship = SelectionHelper.SelectFromListById(
            inactiveSpaceships,
            s => s.SpaceshipId,
            s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
            "Select a spaceship to activate");

        if (selectedSpaceship == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Activating spaceship ID {selectedSpaceship.SpaceshipId}...", async ctx =>
        {
            var result = await _spaceshipService.ActivateAsync(selectedSpaceship.SpaceshipId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Spaceship activated successfully![/]");

                var spaceship = await _spaceshipService.GetByIdAsync(selectedSpaceship.SpaceshipId);
                if (spaceship != null)
                {
                    DisplayEntityDetails(spaceship);
                }
            }
            return true;
        });
    }

    private async Task DeactivateSpaceshipAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var activeSpaceships = await ExecuteWithSpinnerAsync(
            "Fetching active spaceships...",
            async ctx => await _spaceshipService.GetActiveAsync());

        if (!activeSpaceships.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active spaceships found.[/]");
            return;
        }

        var selectedSpaceship = SelectionHelper.SelectFromListById(
            activeSpaceships,
            s => s.SpaceshipId,
            s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
            "Select a spaceship to deactivate");

        if (selectedSpaceship == null)
        {
            return;
        }

        var cancelCruises = InputHelper.AskForConfirmation(
            "Do you want to automatically cancel all scheduled cruises for this spaceship?",
            true);

        await ExecuteWithSpinnerAsync($"Deactivating spaceship ID {selectedSpaceship.SpaceshipId}...", async ctx =>
        {
            var (success, cancelledCruises) = await _spaceshipService.DeactivateAsync(selectedSpaceship.SpaceshipId, cancelCruises);

            if (success)
            {
                AnsiConsole.MarkupLine("[green]Spaceship deactivated successfully![/]");

                if (cancelCruises && cancelledCruises > 0)
                {
                    AnsiConsole.MarkupLine($"[green]{cancelledCruises} scheduled cruise(s) were automatically cancelled.[/]");
                }

                var spaceship = await _spaceshipService.GetByIdAsync(selectedSpaceship.SpaceshipId);
                if (spaceship != null)
                {
                    DisplayEntityDetails(spaceship);
                }
            }
            return success;
        });
    }

    protected override void DisplayEntities(IEnumerable<Spaceship> spaceships, string title)
    {
        var spaceshipList = spaceships.ToList();

        var columns = new[] { "ID", "Model", "Capacity", "Cruise Speed", "Status" };

        var rows = spaceshipList.Select(s => new[]
        {
            s.SpaceshipId.ToString(),
            $"{s.ModelName ?? "Unknown"} (ID: {s.ModelId})",
            s.Capacity?.ToString() ?? "Unknown",
            s.CruiseSpeedKmph.HasValue ? $"{s.CruiseSpeedKmph.Value:N0} kmph" : "Unknown",
            DisplayHelper.FormatActiveStatus(s.IsActive)
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(Spaceship spaceship)
    {
        var details = new Dictionary<string, string>
        {
            ["Spaceship ID"] = spaceship.SpaceshipId.ToString(),
            ["Model"] = $"{spaceship.ModelName ?? "Unknown"} (ID: {spaceship.ModelId})",
            ["Capacity"] = spaceship.Capacity?.ToString() ?? "Unknown",
            ["Cruise Speed"] = spaceship.CruiseSpeedKmph.HasValue ? $"{spaceship.CruiseSpeedKmph.Value:N0} kmph" : "Unknown",
            ["Status"] = DisplayHelper.FormatActiveStatus(spaceship.IsActive)
        };

        DisplayHelper.DisplayDetails("Spaceship Details", details);
    }

    private void DisplayAvailableSpaceships(IEnumerable<SpaceshipAvailability> availableSpaceships, string title)
    {
        var spaceshipList = availableSpaceships.ToList();

        foreach (var spaceship in spaceshipList)
        {
            var timeSlotTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title($"[bold blue]Spaceship #{spaceship.SpaceshipId} - {spaceship.ModelName}[/]")
                .AddColumn(new TableColumn("[u]Available Time Slots[/]"))
                .AddColumn(new TableColumn("[u]Duration[/]"));

            foreach (var slot in spaceship.AvailableTimeSlots)
            {
                var duration = slot.EndTime - slot.StartTime;
                var formattedDuration = duration.TotalHours >= 24
                    ? $"{duration.TotalDays:F1} days"
                    : $"{duration.TotalHours:F1} hours";

                timeSlotTable.AddRow(
                    $"{DisplayHelper.FormatDateTime(slot.StartTime)} to {DisplayHelper.FormatDateTime(slot.EndTime)}",
                    formattedDuration
                );
            }
            AnsiConsole.Write(timeSlotTable);
            AnsiConsole.WriteLine();
        }
    }
}