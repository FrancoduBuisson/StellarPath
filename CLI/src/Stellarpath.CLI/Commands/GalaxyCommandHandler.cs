using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;
using System.Text;

namespace Stellarpath.CLI.Commands;

public class GalaxyCommandHandler : CommandHandlerBase<Galaxy>
{
    private readonly GalaxyService _galaxyService;

    public GalaxyCommandHandler(CommandContext context, GalaxyService galaxyService)
        : base(context)
    {
        _galaxyService = galaxyService;
    }

    protected override string GetMenuTitle() => "Galaxy Management";
    protected override string GetEntityName() => "Galaxy";
    protected override string GetEntityNamePlural() => "Galaxies";

    protected override async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "List All Galaxies":
                await ListAllGalaxiesAsync();
                break;
            case "List Active Galaxies":
                await ListActiveGalaxiesAsync();
                break;
            case "View Galaxy Details":
                await ViewGalaxyDetailsAsync();
                break;
            case "Search Galaxies":
                await SearchGalaxiesAsync();
                break;
            case "Create New Galaxy":
                await CreateGalaxyAsync();
                break;
            case "Update Galaxy":
                await UpdateGalaxyAsync();
                break;
            case "Activate Galaxy":
                await ActivateGalaxyAsync();
                break;
            case "Deactivate Galaxy":
                await DeactivateGalaxyAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ListAllGalaxiesAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching galaxies...", async ctx =>
        {
            var galaxies = await _galaxyService.GetAllAsync();
            DisplayEntities(galaxies, "All Galaxies");
            return true;
        });
    }

    private async Task ListActiveGalaxiesAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching active galaxies...", async ctx =>
        {
            var galaxies = await _galaxyService.GetActiveAsync();
            DisplayEntities(galaxies, "Active Galaxies");
            return true;
        });
    }

    private async Task ViewGalaxyDetailsAsync()
    {
        var galaxy = await FetchAndPromptForEntitySelectionAsync(
            _galaxyService,
            service => service.GetAllAsync(),
            g => g.GalaxyName,
            g => g.GalaxyId,
            "Fetching galaxies for selection...",
            "No galaxies found.",
            "Select a galaxy to view details");

        if (galaxy != null)
        {
            await ExecuteWithSpinnerAsync($"Fetching details for galaxy ID {galaxy.GalaxyId}...", async ctx =>
            {
                var fetchedGalaxy = await _galaxyService.GetByIdAsync(galaxy.GalaxyId);
                if (fetchedGalaxy != null)
                {
                    DisplayEntityDetails(fetchedGalaxy);
                }
                return true;
            });
        }
    }

   private async Task SearchGalaxiesAsync()
{
    var searchCriteria = new GalaxySearchCriteria();

    InputHelper.CollectSearchCriteria<GalaxySearchCriteria>(
        "name",
        criteria => criteria.Name = InputHelper.AskForString("Enter galaxy name (or part of name):"),
        searchCriteria);

    var statusOptions = new List<string> { "All Galaxies", "Active Only", "Inactive Only" };
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
        case "All Galaxies":
        default:
            searchCriteria.IsActive = null;
            break;
    }

    await ExecuteWithSpinnerAsync("Searching galaxies...", async ctx =>
    {
        var galaxies = await _galaxyService.SearchGalaxiesAsync(searchCriteria);

        var title = new StringBuilder("[bold blue]Search Results for Galaxies[/]");
        if (!string.IsNullOrEmpty(searchCriteria.Name))
        {
            title.Append($" with [yellow]name[/] containing '[green]{searchCriteria.Name}[/]'");
        }
        if (searchCriteria.IsActive.HasValue)
        {
            title.Append($" ([yellow]Status[/]: [green]{(searchCriteria.IsActive.Value ? "Active" : "Inactive")}[/])");
        }

        DisplayEntities(galaxies, title.ToString());
        return true;
    });
}

    private async Task CreateGalaxyAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var newGalaxy = new Galaxy
        {
            GalaxyName = InputHelper.AskForString("Enter galaxy name:"),
            IsActive = InputHelper.AskForConfirmation("Should this galaxy be active?", true)
        };

        await ExecuteWithSpinnerAsync("Creating galaxy...", async ctx =>
        {
            var result = await _galaxyService.CreateAsync(newGalaxy);

            if (result.HasValue)
            {
                AnsiConsole.MarkupLine($"[green]Galaxy created successfully with ID: {result.Value}[/]");

                var galaxy = await _galaxyService.GetByIdAsync(result.Value);
                if (galaxy != null)
                {
                    DisplayEntityDetails(galaxy);
                }
            }
            return true;
        });
    }

    private async Task UpdateGalaxyAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var galaxy = await FetchAndPromptForEntitySelectionAsync(
            _galaxyService,
            service => service.GetAllAsync(),
            g => g.GalaxyName,
            g => g.GalaxyId,
            "Fetching galaxies for selection...",
            "No galaxies found to update.",
            "Select a galaxy to update");

        if (galaxy == null)
        {
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Galaxy ID: {galaxy.GalaxyId}[/]");
        AnsiConsole.MarkupLine($"Current Name: [yellow]{galaxy.GalaxyName}[/]");
        AnsiConsole.MarkupLine($"Current Status: [yellow]{(galaxy.IsActive ? "Active" : "Inactive")}[/]");

        galaxy.GalaxyName = InputHelper.AskForString("Enter new name:", galaxy.GalaxyName);
        galaxy.IsActive = InputHelper.AskForConfirmation("Should this galaxy be active?", galaxy.IsActive);

        await ExecuteWithSpinnerAsync("Updating galaxy...", async ctx =>
        {
            var result = await _galaxyService.UpdateAsync(galaxy, galaxy.GalaxyId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Galaxy updated successfully![/]");

                var updatedGalaxy = await _galaxyService.GetByIdAsync(galaxy.GalaxyId);
                if (updatedGalaxy != null)
                {
                    DisplayEntityDetails(updatedGalaxy);
                }
            }
            return true;
        });
    }

    private async Task ActivateGalaxyAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var allGalaxies = await ExecuteWithSpinnerAsync(
            "Fetching inactive galaxies...",
            async ctx => await _galaxyService.GetAllAsync());

        var inactiveGalaxies = allGalaxies.Where(g => !g.IsActive).ToList();

        if (!inactiveGalaxies.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No inactive galaxies found.[/]");
            return;
        }

        var selectedGalaxy = SelectionHelper.SelectFromListById(
            inactiveGalaxies,
            g => g.GalaxyId,
            g => g.GalaxyName,
            "Select a galaxy to activate");

        if (selectedGalaxy == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Activating galaxy ID {selectedGalaxy.GalaxyId}...", async ctx =>
        {
            var result = await _galaxyService.ActivateAsync(selectedGalaxy.GalaxyId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Galaxy activated successfully![/]");

                var galaxy = await _galaxyService.GetByIdAsync(selectedGalaxy.GalaxyId);
                if (galaxy != null)
                {
                    DisplayEntityDetails(galaxy);
                }
            }
            return true;
        });
    }

    private async Task DeactivateGalaxyAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var activeGalaxies = await ExecuteWithSpinnerAsync(
            "Fetching active galaxies...",
            async ctx => await _galaxyService.GetActiveAsync());

        if (!activeGalaxies.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active galaxies found.[/]");
            return;
        }

        var selectedGalaxy = SelectionHelper.SelectFromListById(
            activeGalaxies,
            g => g.GalaxyId,
            g => g.GalaxyName,
            "Select a galaxy to deactivate");

        if (selectedGalaxy == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Deactivating galaxy ID {selectedGalaxy.GalaxyId}...", async ctx =>
        {
            var result = await _galaxyService.DeactivateAsync(selectedGalaxy.GalaxyId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Galaxy deactivated successfully![/]");

                var galaxy = await _galaxyService.GetByIdAsync(selectedGalaxy.GalaxyId);
                if (galaxy != null)
                {
                    DisplayEntityDetails(galaxy);
                }
            }
            return true;
        });
    }

    protected override void DisplayEntities(IEnumerable<Galaxy> galaxies, string title)
    {
        var galaxyList = galaxies.ToList();

        var columns = new[] { "ID", "Name", "Status" };

        var rows = galaxyList.Select(g => new[]
        {
            g.GalaxyId.ToString(),
            g.GalaxyName,
            DisplayHelper.FormatActiveStatus(g.IsActive)
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(Galaxy galaxy)
    {
        var details = new Dictionary<string, string>
        {
            ["Galaxy ID"] = galaxy.GalaxyId.ToString(),
            ["Name"] = galaxy.GalaxyName,
            ["Status"] = DisplayHelper.FormatActiveStatus(galaxy.IsActive)
        };

        DisplayHelper.DisplayDetails("Galaxy Details", details);
    }
}