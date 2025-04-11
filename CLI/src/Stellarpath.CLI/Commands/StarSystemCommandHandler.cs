using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class StarSystemCommandHandler
{
    private readonly CommandContext _context;
    private readonly StarSystemService _starSystemService;
    private readonly GalaxyService _galaxyService;

    public StarSystemCommandHandler(CommandContext context, StarSystemService starSystemService, GalaxyService galaxyService)
    {
        _context = context;
        _starSystemService = starSystemService;
        _galaxyService = galaxyService;
    }

    public async Task HandleAsync()
    {
        var options = new List<string>
        {
            "List All Star Systems",
            "List Active Star Systems",
            "View Star System Details",
            "Search Star Systems",
            "View Star Systems by Galaxy"
        };

        if (_context.CurrentUser?.Role == "Admin")
        {
            options.AddRange(new[]
            {
                "Create New Star System",
                "Update Star System",
                "Activate Star System",
                "Deactivate Star System"
            });
        }

        options.Add("Back to Main Menu");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Star System Management")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        await ProcessSelectionAsync(selection);
    }

    private async Task ProcessSelectionAsync(string selection)
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
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching star systems...", async ctx =>
            {
                var starSystems = await _starSystemService.GetAllAsync();
                DisplayStarSystems(starSystems, "All Star Systems");
            });
    }

    private async Task ListActiveStarSystemsAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active star systems...", async ctx =>
            {
                var starSystems = await _starSystemService.GetActiveAsync();
                DisplayStarSystems(starSystems, "Active Star Systems");
            });
    }

    private async Task ViewStarSystemDetailsAsync()
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

                ctx.Status("Select a star system to view details");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allStarSystems = await _starSystemService.GetAllAsync();
        if (!allStarSystems.Any())
        {
            return;
        }

        var starSystemNames = allStarSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
        var selectedStarSystemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a star system to view details")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(starSystemNames));

        int systemId = int.Parse(selectedStarSystemName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching details for star system ID {systemId}...", async ctx =>
            {
                var starSystem = await _starSystemService.GetByIdAsync(systemId);

                if (starSystem != null)
                {
                    DisplayStarSystemDetails(starSystem);
                }
            });
    }

    private async Task SearchStarSystemsAsync()
    {
        var searchCriteria = new StarSystemSearchCriteria();

        var includeName = AnsiConsole.Confirm("Do you want to search by name?", false);
        if (includeName)
        {
            searchCriteria.Name = AnsiConsole.Ask<string>("Enter star system name (or part of name):");
        }

        var includeGalaxy = AnsiConsole.Confirm("Do you want to filter by galaxy?", false);
        if (includeGalaxy)
        {
            var byGalaxyId = AnsiConsole.Confirm("Search by galaxy ID? (No = search by galaxy name)", false);
            if (byGalaxyId)
            {
                searchCriteria.GalaxyId = AnsiConsole.Ask<int>("Enter galaxy ID:");
            }
            else
            {
                searchCriteria.GalaxyName = AnsiConsole.Ask<string>("Enter galaxy name (or part of name):");
            }
        }

        var includeStatus = AnsiConsole.Confirm("Do you want to filter by active status?", false);
        if (includeStatus)
        {
            searchCriteria.IsActive = AnsiConsole.Confirm("Show only active star systems?");
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Searching star systems...", async ctx =>
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

                DisplayStarSystems(starSystems, title);
            });
    }

    private async Task ViewStarSystemsByGalaxyAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching galaxies for selection...", async ctx =>
            {
                var galaxies = await _galaxyService.GetAllAsync();

                if (!galaxies.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No galaxies found.[/]");
                    return;
                }

                ctx.Status("Select a galaxy to view its star systems");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var galaxies = await _galaxyService.GetAllAsync();
        if (!galaxies.Any())
        {
            return;
        }

        var galaxyNames = galaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
        var selectedGalaxyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a galaxy to view its star systems")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(galaxyNames));

        int galaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching star systems for galaxy ID {galaxyId}...", async ctx =>
            {
                var starSystems = await _starSystemService.GetStarSystemsByGalaxyIdAsync(galaxyId);

                var galaxyName = galaxies.FirstOrDefault(g => g.GalaxyId == galaxyId)?.GalaxyName ?? $"ID: {galaxyId}";
                DisplayStarSystems(starSystems, $"Star Systems in Galaxy: {galaxyName}");
            });
    }

    private async Task CreateStarSystemAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to create star systems.[/]");
            return;
        }

        var galaxies = await _galaxyService.GetActiveAsync();
        if (!galaxies.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active galaxies found. Please create a galaxy first.[/]");
            return;
        }

        var galaxyNames = galaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
        var selectedGalaxyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select the galaxy for this star system")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(galaxyNames));

        int galaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());

        var newStarSystem = new StarSystem
        {
            SystemName = AnsiConsole.Ask<string>("Enter star system name:"),
            GalaxyId = galaxyId,
            IsActive = AnsiConsole.Confirm("Should this star system be active?", true)
        };

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Creating star system...", async ctx =>
            {
                var result = await _starSystemService.CreateAsync(newStarSystem);

                if (result.HasValue)
                {
                    AnsiConsole.MarkupLine($"[green]Star system created successfully with ID: {result.Value}[/]");

                    var starSystem = await _starSystemService.GetByIdAsync(result.Value);
                    if (starSystem != null)
                    {
                        DisplayStarSystemDetails(starSystem);
                    }
                }
            });
    }

    private async Task UpdateStarSystemAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to update star systems.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching star systems for selection...", async ctx =>
            {
                var starSystems = await _starSystemService.GetAllAsync();

                if (!starSystems.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No star systems found to update.[/]");
                    return;
                }

                ctx.Status("Select a star system to update");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allStarSystems = await _starSystemService.GetAllAsync();
        if (!allStarSystems.Any())
        {
            return;
        }

        var starSystemNames = allStarSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
        var selectedStarSystemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a star system to update")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(starSystemNames));

        int systemId = int.Parse(selectedStarSystemName.Split(':')[0].Trim());

        var starSystem = await _starSystemService.GetByIdAsync(systemId);
        if (starSystem == null)
        {
            return;
        }

        var galaxies = await _galaxyService.GetAllAsync();
        if (!galaxies.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No galaxies found. Cannot update galaxy reference.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Star System ID: {starSystem.SystemId}[/]");
        AnsiConsole.MarkupLine($"Current Name: [yellow]{starSystem.SystemName}[/]");
        AnsiConsole.MarkupLine($"Current Galaxy: [yellow]{starSystem.GalaxyName} (ID: {starSystem.GalaxyId})[/]");
        AnsiConsole.MarkupLine($"Current Status: [yellow]{(starSystem.IsActive ? "Active" : "Inactive")}[/]");

        starSystem.SystemName = AnsiConsole.Ask("Enter new name:", starSystem.SystemName);

        var changeGalaxy = AnsiConsole.Confirm("Do you want to change the galaxy?", false);
        if (changeGalaxy)
        {
            var galaxyNames = galaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
            var selectedGalaxyName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the new galaxy for this star system")
                    .PageSize(10)
                    .HighlightStyle(new Style(Color.Green))
                    .AddChoices(galaxyNames));

            starSystem.GalaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());
        }

        starSystem.IsActive = AnsiConsole.Confirm("Should this star system be active?", starSystem.IsActive);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Updating star system...", async ctx =>
            {
                var result = await _starSystemService.UpdateAsync(starSystem, starSystem.SystemId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Star system updated successfully![/]");

                    var updatedStarSystem = await _starSystemService.GetByIdAsync(starSystem.SystemId);
                    if (updatedStarSystem != null)
                    {
                        DisplayStarSystemDetails(updatedStarSystem);
                    }
                }
            });
    }

    private async Task ActivateStarSystemAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to activate star systems.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching inactive star systems...", async ctx =>
            {
                var allStarSystems = await _starSystemService.GetAllAsync();
                var inactiveStarSystems = allStarSystems.Where(s => !s.IsActive).ToList();

                if (!inactiveStarSystems.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No inactive star systems found.[/]");
                    return;
                }

                ctx.Status("Select a star system to activate");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allStarSystems = await _starSystemService.GetAllAsync();
        var inactiveStarSystems = allStarSystems.Where(s => !s.IsActive).ToList();

        if (!inactiveStarSystems.Any())
        {
            return;
        }

        var starSystemNames = inactiveStarSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
        var selectedStarSystemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a star system to activate")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(starSystemNames));

        int systemId = int.Parse(selectedStarSystemName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Activating star system ID {systemId}...", async ctx =>
            {
                var result = await _starSystemService.ActivateAsync(systemId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Star system activated successfully![/]");

                    var starSystem = await _starSystemService.GetByIdAsync(systemId);
                    if (starSystem != null)
                    {
                        DisplayStarSystemDetails(starSystem);
                    }
                }
            });
    }

    private async Task DeactivateStarSystemAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to deactivate star systems.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active star systems...", async ctx =>
            {
                var activeStarSystems = await _starSystemService.GetActiveAsync();

                if (!activeStarSystems.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No active star systems found.[/]");
                    return;
                }

                ctx.Status("Select a star system to deactivate");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var activeStarSystems = await _starSystemService.GetActiveAsync();

        if (!activeStarSystems.Any())
        {
            return;
        }

        var starSystemNames = activeStarSystems.Select(s => $"{s.SystemId}: {s.SystemName}").ToList();
        var selectedStarSystemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a star system to deactivate")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(starSystemNames));

        int systemId = int.Parse(selectedStarSystemName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Deactivating star system ID {systemId}...", async ctx =>
            {
                var result = await _starSystemService.DeactivateAsync(systemId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Star system deactivated successfully![/]");

                    var starSystem = await _starSystemService.GetByIdAsync(systemId);
                    if (starSystem != null)
                    {
                        DisplayStarSystemDetails(starSystem);
                    }
                }
            });
    }

    private void DisplayStarSystems(IEnumerable<StarSystem> starSystems, string title)
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

    private void DisplayStarSystemDetails(StarSystem starSystem)
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