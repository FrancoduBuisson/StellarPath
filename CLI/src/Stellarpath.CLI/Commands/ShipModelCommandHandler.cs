using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class ShipModelCommandHandler : CommandHandlerBase<ShipModel>
{
    private readonly ShipModelService _shipModelService;

    public ShipModelCommandHandler(CommandContext context, ShipModelService shipModelService)
        : base(context)
    {
        _shipModelService = shipModelService;
    }

    protected override string GetMenuTitle() => "Ship Model Management";
    protected override string GetEntityName() => "Ship Model";
    protected override string GetEntityNamePlural() => "Ship Models";

    protected override async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "List All Ship Models":
                await ListAllShipModelsAsync();
                break;
            case "View Ship Model Details":
                await ViewShipModelDetailsAsync();
                break;
            case "Search Ship Models":
                await SearchShipModelsAsync();
                break;
            case "Create New Ship Model":
                await CreateShipModelAsync();
                break;
            case "Update Ship Model":
                await UpdateShipModelAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    protected override List<string> GetBaseOptions()
    {
        return new List<string>
        {
            $"List All {GetEntityNamePlural()}",
            $"View {GetEntityName()} Details",
            $"Search {GetEntityNamePlural()}"
        };
    }

    protected override List<string> GetAdminOptions()
    {
        return new List<string>
        {
            $"Create New {GetEntityName()}",
            $"Update {GetEntityName()}"
        };
    }

    private async Task ListAllShipModelsAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching ship models...", async ctx =>
        {
            var shipModels = await _shipModelService.GetAllAsync();
            DisplayEntities(shipModels, "All Ship Models");
            return true;
        });
    }

    private async Task ViewShipModelDetailsAsync()
    {
        var shipModel = await FetchAndPromptForEntitySelectionAsync<ShipModelService, ShipModel>(
            _shipModelService,
            service => service.GetAllAsync(),
            m => m.ModelName,
            m => m.ModelId,
            "Fetching ship models for selection...",
            "No ship models found.",
            "Select a ship model to view details");

        if (shipModel != null)
        {
            await ExecuteWithSpinnerAsync($"Fetching details for ship model ID {shipModel.ModelId}...", async ctx =>
            {
                var fetchedShipModel = await _shipModelService.GetByIdAsync(shipModel.ModelId);
                if (fetchedShipModel != null)
                {
                    DisplayEntityDetails(fetchedShipModel);
                }
                return true;
            });
        }
    }

    private async Task SearchShipModelsAsync()
    {
        var searchCriteria = new ShipModelSearchCriteria();

        InputHelper.CollectSearchCriteria<ShipModelSearchCriteria>(
            "name",
            criteria => criteria.Name = InputHelper.AskForString("Enter ship model name (or part of name):"),
            searchCriteria);

        var includeCapacity = InputHelper.AskForConfirmation("Do you want to filter by passenger capacity?", false);
        if (includeCapacity)
        {
            var includeMinCapacity = InputHelper.AskForConfirmation("Set minimum capacity?", false);
            if (includeMinCapacity)
            {
                searchCriteria.MinCapacity = InputHelper.AskForInt("Enter minimum passenger capacity:", min: 1);
            }

            var includeMaxCapacity = InputHelper.AskForConfirmation("Set maximum capacity?", false);
            if (includeMaxCapacity)
            {
                searchCriteria.MaxCapacity = InputHelper.AskForInt("Enter maximum passenger capacity:", min: searchCriteria.MinCapacity ?? 1);
            }
        }

        var includeSpeed = InputHelper.AskForConfirmation("Do you want to filter by cruise speed?", false);
        if (includeSpeed)
        {
            var includeMinSpeed = InputHelper.AskForConfirmation("Set minimum cruise speed?", false);
            if (includeMinSpeed)
            {
                searchCriteria.MinSpeed = InputHelper.AskForInt("Enter minimum cruise speed (kmph):", min: 1);
            }

            var includeMaxSpeed = InputHelper.AskForConfirmation("Set maximum cruise speed?", false);
            if (includeMaxSpeed)
            {
                searchCriteria.MaxSpeed = InputHelper.AskForInt("Enter maximum cruise speed (kmph):", min: searchCriteria.MinSpeed ?? 1);
            }
        }

        await ExecuteWithSpinnerAsync("Searching ship models...", async ctx =>
        {
            var shipModels = await _shipModelService.SearchShipModelsAsync(searchCriteria);

            string title = "Search Results for Ship Models";
            if (!string.IsNullOrEmpty(searchCriteria.Name))
            {
                title += $" containing '{searchCriteria.Name}'";
            }
            if (searchCriteria.MinCapacity.HasValue)
            {
                title += $" with capacity ≥ {searchCriteria.MinCapacity.Value}";
            }
            if (searchCriteria.MaxCapacity.HasValue)
            {
                title += $" with capacity ≤ {searchCriteria.MaxCapacity.Value}";
            }
            if (searchCriteria.MinSpeed.HasValue)
            {
                title += $" with speed ≥ {searchCriteria.MinSpeed.Value} kmph";
            }
            if (searchCriteria.MaxSpeed.HasValue)
            {
                title += $" with speed ≤ {searchCriteria.MaxSpeed.Value} kmph";
            }

            DisplayEntities(shipModels, title);
            return true;
        });
    }

    private async Task CreateShipModelAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var newShipModel = new ShipModel
        {
            ModelName = InputHelper.AskForString("Enter ship model name:"),
            Capacity = InputHelper.AskForInt("Enter passenger capacity:", min: 1),
            CruiseSpeedKmph = InputHelper.AskForInt("Enter cruise speed (kmph):", min: 1)
        };

        await ExecuteWithSpinnerAsync("Creating ship model...", async ctx =>
        {
            var result = await _shipModelService.CreateAsync(newShipModel);

            if (result.HasValue)
            {
                AnsiConsole.MarkupLine($"[green]Ship model created successfully with ID: {result.Value}[/]");

                var shipModel = await _shipModelService.GetByIdAsync(result.Value);
                if (shipModel != null)
                {
                    DisplayEntityDetails(shipModel);
                }
            }
            return true;
        });
    }

    private async Task UpdateShipModelAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var shipModel = await FetchAndPromptForEntitySelectionAsync<ShipModelService, ShipModel>(
            _shipModelService,
            service => service.GetAllAsync(),
            m => m.ModelName,
            m => m.ModelId,
            "Fetching ship models for selection...",
            "No ship models found to update.",
            "Select a ship model to update");

        if (shipModel == null)
        {
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Ship Model ID: {shipModel.ModelId}[/]");
        AnsiConsole.MarkupLine($"Current Name: [yellow]{shipModel.ModelName}[/]");
        AnsiConsole.MarkupLine($"Current Passenger Capacity: [yellow]{shipModel.Capacity}[/]");
        AnsiConsole.MarkupLine($"Current Cruise Speed: [yellow]{shipModel.CruiseSpeedKmph} kmph[/]");

        shipModel.ModelName = InputHelper.AskForString("Enter new name:", shipModel.ModelName);
        shipModel.Capacity = InputHelper.AskForInt("Enter new passenger capacity:", shipModel.Capacity, min: 1);
        shipModel.CruiseSpeedKmph = InputHelper.AskForInt("Enter new cruise speed (kmph):", shipModel.CruiseSpeedKmph, min: 1);

        await ExecuteWithSpinnerAsync("Updating ship model...", async ctx =>
        {
            var result = await _shipModelService.UpdateAsync(shipModel, shipModel.ModelId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Ship model updated successfully![/]");

                var updatedShipModel = await _shipModelService.GetByIdAsync(shipModel.ModelId);
                if (updatedShipModel != null)
                {
                    DisplayEntityDetails(updatedShipModel);
                }
            }
            return true;
        });
    }

    protected override void DisplayEntities(IEnumerable<ShipModel> shipModels, string title)
    {
        var shipModelList = shipModels.ToList();

        var columns = new[] { "ID", "Name", "Passenger Capacity", "Cruise Speed" };

        var rows = shipModelList.Select(m => new[]
        {
            m.ModelId.ToString(),
            m.ModelName,
            m.Capacity.ToString(),
            $"{m.CruiseSpeedKmph:N0} kmph"
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(ShipModel shipModel)
    {
        var details = new Dictionary<string, string>
        {
            ["Ship Model ID"] = shipModel.ModelId.ToString(),
            ["Name"] = shipModel.ModelName,
            ["Passenger Capacity"] = shipModel.Capacity.ToString(),
            ["Cruise Speed"] = $"{shipModel.CruiseSpeedKmph:N0} kmph"
        };

        DisplayHelper.DisplayDetails("Ship Model Details", details);
    }
}