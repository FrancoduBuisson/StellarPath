using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class StarSystemCommandHandler : CommandHandlerBase<StarSystem>
{
    private readonly StarSystemService _starSystemService;
    private readonly GalaxyService _galaxyService;

    public StarSystemCommandHandler(CommandContext context, StarSystemService starSystemService, GalaxyService galaxyService)
        : base(context)
    {
        _starSystemService = starSystemService;
        _galaxyService = galaxyService;
    }

    protected override string GetMenuTitle() => "Star System Management";
    protected override string GetEntityName() => "Star System";
    protected override string GetEntityNamePlural() => "Star Systems";

    protected override List<string> GetEntitySpecificOptions()
    {
        return new List<string>
        {
            "View Star Systems by Galaxy"
        };
    }

    protected override async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "List All Star Systems":
                await ListAllStarSystemsAsync();
                break;
            case "List Active Star Systems":
                await ListActiveStarSystemsAsync();
                break;
            case "View Star System Details":
                await ViewStarSystemDetailsAsync();
                break;
            case "Search Star Systems":
                await SearchStarSystemsAsync();
                break;
            case "View Star Systems by Galaxy":
                await ViewStarSystemsByGalaxyAsync();
                break;
            case "Create New Star System":
                await CreateStarSystemAsync();
                break;
            case "Update Star System":
                await UpdateStarSystemAsync();
                break;
            case "Activate Star System":
                await ActivateStarSystemAsync();
                break;
            case "Deactivate Star System":
                await DeactivateStarSystemAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ListAllStarSystemsAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching star systems...", async ctx =>
        {
            var starSystems = await _starSystemService.GetAllAsync();
            DisplayEntities(starSystems, "All Star Systems");
            return true;
        });
    }

    private async Task ListActiveStarSystemsAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching active star systems...", async ctx =>
        {
            var starSystems = await _starSystemService.GetActiveAsync();
            DisplayEntities(starSystems, "Active Star Systems");
            return true;
        });
    }

    private async Task ViewStarSystemDetailsAsync()
    {
        var starSystem = await FetchAndPromptForEntitySelectionAsync(
            _starSystemService,
            service => service.GetAllAsync(),
            s => s.SystemName,
            s => s.SystemId,
            "Fetching star systems for selection...",
            "No star systems found.",
            "Select a star system to view details");

        if (starSystem != null)
        {
            await ExecuteWithSpinnerAsync($"Fetching details for star system ID {starSystem.SystemId}...", async ctx =>
            {
                var fetchedStarSystem = await _starSystemService.GetByIdAsync(starSystem.SystemId);
                if (fetchedStarSystem != null)
                {
                    DisplayEntityDetails(fetchedStarSystem);
                }
                return true;
            });
        }
    }

    private async Task SearchStarSystemsAsync()
    {
        var searchCriteria = new StarSystemSearchCriteria();

        InputHelper.CollectSearchCriteria<StarSystemSearchCriteria>(
            "name",
            criteria => criteria.Name = InputHelper.AskForString("Enter star system name (or part of name):"),
            searchCriteria);

        var includeGalaxy = InputHelper.AskForConfirmation("Do you want to filter by galaxy?", false);
        if (includeGalaxy)
        {
            var byGalaxyId = InputHelper.AskForConfirmation("Search by galaxy ID? (No = search by galaxy name)", false);
            if (byGalaxyId)
            {
                searchCriteria.GalaxyId = InputHelper.AskForInt("Enter galaxy ID:");
            }
            else
            {
                searchCriteria.GalaxyName = InputHelper.AskForString("Enter galaxy name (or part of name):");
            }
        }

        InputHelper.CollectSearchCriteria<StarSystemSearchCriteria>(
            "active status",
            criteria => criteria.IsActive = InputHelper.AskForConfirmation("Show only active star systems?"),
            searchCriteria);

        await ExecuteWithSpinnerAsync("Searching star systems...", async ctx =>
        {
            var starSystems = await _starSystemService.SearchStarSystemsAsync(searchCriteria);

            string title = "Search Results for Star Systems";
            if (!string.IsNullOrEmpty(searchCriteria.Name))
            {
                title += $" containing '{searchCriteria.Name}'";
            }
            if (searchCriteria.GalaxyId.HasValue)
            {
                title += $" in galaxy ID {searchCriteria.GalaxyId.Value}";
            }
            if (!string.IsNullOrEmpty(searchCriteria.GalaxyName))
            {
                title += $" in galaxy '{searchCriteria.GalaxyName}'";
            }
            if (searchCriteria.IsActive.HasValue)
            {
                title += $" (Status: {(searchCriteria.IsActive.Value ? "Active" : "Inactive")})";
            }

            DisplayEntities(starSystems, title);
            return true;
        });
    }

    private async Task ViewStarSystemsByGalaxyAsync()
    {
        var galaxy = await FetchAndPromptForEntitySelectionAsync(
            _galaxyService,
            service => service.GetAllAsync(),
            g => g.GalaxyName,
            g => g.GalaxyId,
            "Fetching galaxies for selection...",
            "No galaxies found.",
            "Select a galaxy to view its star systems");

        if (galaxy == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching star systems for galaxy ID {galaxy.GalaxyId}...", async ctx =>
        {
            var starSystems = await _starSystemService.GetStarSystemsByGalaxyIdAsync(galaxy.GalaxyId);
            DisplayEntities(starSystems, $"Star Systems in Galaxy: {galaxy.GalaxyName}");
            return true;
        });
    }

    private async Task CreateStarSystemAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        // First get active galaxies for selection
        var galaxies = await ExecuteWithSpinnerAsync(
            "Fetching active galaxies for selection...",
            async ctx => await _galaxyService.GetActiveAsync());

        if (!galaxies.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active galaxies found. Please create a galaxy first.[/]");
            return;
        }

        var selectedGalaxy = SelectionHelper.SelectFromListById(
            galaxies,
            g => g.GalaxyId,
            g => g.GalaxyName,
            "Select the galaxy for this star system");

        if (selectedGalaxy == null)
        {
            return;
        }

        var newStarSystem = new StarSystem
        {
            SystemName = InputHelper.AskForString("Enter star system name:"),
            GalaxyId = selectedGalaxy.GalaxyId,
            IsActive = InputHelper.AskForConfirmation("Should this star system be active?", true)
        };

        await ExecuteWithSpinnerAsync("Creating star system...", async ctx =>
        {
            var result = await _starSystemService.CreateAsync(newStarSystem);

            if (result.HasValue)
            {
                AnsiConsole.MarkupLine($"[green]Star system created successfully with ID: {result.Value}[/]");

                var starSystem = await _starSystemService.GetByIdAsync(result.Value);
                if (starSystem != null)
                {
                    DisplayEntityDetails(starSystem);
                }
            }
            return true;
        });
    }

    private async Task UpdateStarSystemAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var starSystem = await FetchAndPromptForEntitySelectionAsync(
            _starSystemService,
            service => service.GetAllAsync(),
            s => s.SystemName,
            s => s.SystemId,
            "Fetching star systems for selection...",
            "No star systems found to update.",
            "Select a star system to update");

        if (starSystem == null)
        {
            return;
        }

        // Also fetch available galaxies
        var galaxies = await ExecuteWithSpinnerAsync(
            "Fetching galaxies...",
            async ctx => await _galaxyService.GetAllAsync());

        if (!galaxies.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No galaxies found. Cannot update galaxy reference.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Star System ID: {starSystem.SystemId}[/]");
        AnsiConsole.MarkupLine($"Current Name: [yellow]{starSystem.SystemName}[/]");
        AnsiConsole.MarkupLine($"Current Galaxy: [yellow]{starSystem.GalaxyName} (ID: {starSystem.GalaxyId})[/]");
        AnsiConsole.MarkupLine($"Current Status: [yellow]{(starSystem.IsActive ? "Active" : "Inactive")}[/]");

        starSystem.SystemName = InputHelper.AskForString("Enter new name:", starSystem.SystemName);

        var changeGalaxy = InputHelper.AskForConfirmation("Do you want to change the galaxy?", false);
        if (changeGalaxy)
        {
            var selectedGalaxy = SelectionHelper.SelectFromListById(
                galaxies,
                g => g.GalaxyId,
                g => g.GalaxyName,
                "Select the new galaxy for this star system");

            if (selectedGalaxy != null)
            {
                starSystem.GalaxyId = selectedGalaxy.GalaxyId;
            }
        }

        starSystem.IsActive = InputHelper.AskForConfirmation("Should this star system be active?", starSystem.IsActive);

        await ExecuteWithSpinnerAsync("Updating star system...", async ctx =>
        {
            var result = await _starSystemService.UpdateAsync(starSystem, starSystem.SystemId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Star system updated successfully![/]");

                var updatedStarSystem = await _starSystemService.GetByIdAsync(starSystem.SystemId);
                if (updatedStarSystem != null)
                {
                    DisplayEntityDetails(updatedStarSystem);
                }
            }
            return true;
        });
    }

    private async Task ActivateStarSystemAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var allStarSystems = await ExecuteWithSpinnerAsync(
            "Fetching inactive star systems...",
            async ctx => await _starSystemService.GetAllAsync());

        var inactiveStarSystems = allStarSystems.Where(s => !s.IsActive).ToList();

        if (!inactiveStarSystems.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No inactive star systems found.[/]");
            return;
        }

        var selectedStarSystem = SelectionHelper.SelectFromListById(
            inactiveStarSystems,
            s => s.SystemId,
            s => s.SystemName,
            "Select a star system to activate");

        if (selectedStarSystem == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Activating star system ID {selectedStarSystem.SystemId}...", async ctx =>
        {
            try
            {
                var result = await _starSystemService.ActivateAsync(selectedStarSystem.SystemId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Star system activated successfully![/]");

                    var starSystem = await _starSystemService.GetByIdAsync(selectedStarSystem.SystemId);
                    if (starSystem != null)
                    {
                        DisplayEntityDetails(starSystem);
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("400")){
                AnsiConsole.MarkupLine("[red]Cannot activate this star system.[/]");
                AnsiConsole.MarkupLine("[yellow]The parent galaxy is inactive. Please activate the galaxy first.[/]");
                return false;
            }
            
            return true;
        });
    }

    private async Task DeactivateStarSystemAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var activeStarSystems = await ExecuteWithSpinnerAsync(
            "Fetching active star systems...",
            async ctx => await _starSystemService.GetActiveAsync());

        if (!activeStarSystems.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active star systems found.[/]");
            return;
        }

        var selectedStarSystem = SelectionHelper.SelectFromListById(
            activeStarSystems,
            s => s.SystemId,
            s => s.SystemName,
            "Select a star system to deactivate");

        if (selectedStarSystem == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Deactivating star system ID {selectedStarSystem.SystemId}...", async ctx =>
        {
            var result = await _starSystemService.DeactivateAsync(selectedStarSystem.SystemId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Star system deactivated successfully![/]");

                var starSystem = await _starSystemService.GetByIdAsync(selectedStarSystem.SystemId);
                if (starSystem != null)
                {
                    DisplayEntityDetails(starSystem);
                }
            }
            return true;
        });
    }

    protected override void DisplayEntities(IEnumerable<StarSystem> starSystems, string title)
    {
        var starSystemList = starSystems.ToList();

        var columns = new[] { "ID", "Name", "Galaxy", "Status" };

        var rows = starSystemList.Select(s => new[]
        {
            s.SystemId.ToString(),
            s.SystemName,
            $"{s.GalaxyName} (ID: {s.GalaxyId})",
            DisplayHelper.FormatActiveStatus(s.IsActive)
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(StarSystem starSystem)
    {
        var details = new Dictionary<string, string>
        {
            ["Star System ID"] = starSystem.SystemId.ToString(),
            ["Name"] = starSystem.SystemName,
            ["Galaxy"] = $"{starSystem.GalaxyName} (ID: {starSystem.GalaxyId})",
            ["Status"] = DisplayHelper.FormatActiveStatus(starSystem.IsActive)
        };

        DisplayHelper.DisplayDetails("Star System Details", details);
    }
}