using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class GalaxyCommandHandler
{
    private readonly CommandContext _context;
    private readonly GalaxyService _galaxyService;

    public GalaxyCommandHandler(CommandContext context, GalaxyService galaxyService)
    {
        _context = context;
        _galaxyService = galaxyService;
    }

    public async Task HandleAsync()
    {
        var options = new List<string>
        {
            "List All Galaxies",
            "List Active Galaxies",
            "View Galaxy Details",
            "Search Galaxies"
        };

        if (_context.CurrentUser?.Role == "Admin")
        {
            options.AddRange(new[]
            {
                "Create New Galaxy",
                "Update Galaxy",
                "Activate Galaxy",
                "Deactivate Galaxy"
            });
        }

        options.Add("Back to Main Menu");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Galaxy Management")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        await ProcessSelectionAsync(selection);
    }

    private async Task ProcessSelectionAsync(string selection)
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
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching galaxies...", async ctx =>
            {
                var galaxies = await _galaxyService.GetAllGalaxiesAsync();
                DisplayGalaxies(galaxies, "All Galaxies");
            });
    }

    private async Task ListActiveGalaxiesAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active galaxies...", async ctx =>
            {
                var galaxies = await _galaxyService.GetActiveGalaxiesAsync();
                DisplayGalaxies(galaxies, "Active Galaxies");
            });
    }

    private async Task ViewGalaxyDetailsAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching galaxies for selection...", async ctx =>
            {
                var galaxies = await _galaxyService.GetAllGalaxiesAsync();

                if (!galaxies.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No galaxies found.[/]");
                    return;
                }

                ctx.Status("Select a galaxy to view details");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allGalaxies = await _galaxyService.GetAllGalaxiesAsync();
        if (!allGalaxies.Any())
        {
            return;
        }

        var galaxyNames = allGalaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
        var selectedGalaxyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a galaxy to view details")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(galaxyNames));

        int galaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching details for galaxy ID {galaxyId}...", async ctx =>
            {
                var galaxy = await _galaxyService.GetGalaxyByIdAsync(galaxyId);

                if (galaxy != null)
                {
                    DisplayGalaxyDetails(galaxy);
                }
            });
    }

    private async Task SearchGalaxiesAsync()
    {
        var searchCriteria = new GalaxySearchCriteria();

        var includeName = AnsiConsole.Confirm("Do you want to search by name?", false);
        if (includeName)
        {
            searchCriteria.Name = AnsiConsole.Ask<string>("Enter galaxy name (or part of name):");
        }

        var includeStatus = AnsiConsole.Confirm("Do you want to filter by active status?", false);
        if (includeStatus)
        {
            searchCriteria.IsActive = AnsiConsole.Confirm("Show only active galaxies?");
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Searching galaxies...", async ctx =>
            {
                var galaxies = await _galaxyService.SearchGalaxiesAsync(searchCriteria);

                string title = "Search Results for Galaxies";
                if (!string.IsNullOrEmpty(searchCriteria.Name))
                {
                    title += $" containing '{searchCriteria.Name}'";
                }
                if (searchCriteria.IsActive.HasValue)
                {
                    title += $" (Status: {(searchCriteria.IsActive.Value ? "Active" : "Inactive")})";
                }

                DisplayGalaxies(galaxies, title);
            });
    }

    private async Task CreateGalaxyAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to create galaxies.[/]");
            return;
        }

        var newGalaxy = new Galaxy
        {
            GalaxyName = AnsiConsole.Ask<string>("Enter galaxy name:"),
            IsActive = AnsiConsole.Confirm("Should this galaxy be active?", true)
        };

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Creating galaxy...", async ctx =>
            {
                var result = await _galaxyService.CreateGalaxyAsync(newGalaxy);

                if (result.HasValue)
                {
                    AnsiConsole.MarkupLine($"[green]Galaxy created successfully with ID: {result.Value}[/]");

                    var galaxy = await _galaxyService.GetGalaxyByIdAsync(result.Value);
                    if (galaxy != null)
                    {
                        DisplayGalaxyDetails(galaxy);
                    }
                }
            });
    }

    private async Task UpdateGalaxyAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to update galaxies.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching galaxies for selection...", async ctx =>
            {
                var galaxies = await _galaxyService.GetAllGalaxiesAsync();

                if (!galaxies.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No galaxies found to update.[/]");
                    return;
                }

                ctx.Status("Select a galaxy to update");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allGalaxies = await _galaxyService.GetAllGalaxiesAsync();
        if (!allGalaxies.Any())
        {
            return;
        }

        var galaxyNames = allGalaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
        var selectedGalaxyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a galaxy to update")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(galaxyNames));

        int galaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching details for galaxy ID {galaxyId}...", async ctx =>
            {
                var galaxy = await _galaxyService.GetGalaxyByIdAsync(galaxyId);

                if (galaxy == null)
                {
                    return;
                }

                ctx.Status("Update galaxy details");
            });

        var galaxy = await _galaxyService.GetGalaxyByIdAsync(galaxyId);
        if (galaxy == null)
        {
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Updating Galaxy ID: {galaxy.GalaxyId}[/]");
        AnsiConsole.MarkupLine($"Current Name: [yellow]{galaxy.GalaxyName}[/]");
        AnsiConsole.MarkupLine($"Current Status: [yellow]{(galaxy.IsActive ? "Active" : "Inactive")}[/]");

        galaxy.GalaxyName = AnsiConsole.Ask("Enter new name:", galaxy.GalaxyName);
        galaxy.IsActive = AnsiConsole.Confirm("Should this galaxy be active?", galaxy.IsActive);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Updating galaxy...", async ctx =>
            {
                var result = await _galaxyService.UpdateGalaxyAsync(galaxy);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Galaxy updated successfully![/]");

                    var updatedGalaxy = await _galaxyService.GetGalaxyByIdAsync(galaxy.GalaxyId);
                    if (updatedGalaxy != null)
                    {
                        DisplayGalaxyDetails(updatedGalaxy);
                    }
                }
            });
    }

    private async Task ActivateGalaxyAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to activate galaxies.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching inactive galaxies...", async ctx =>
            {
                var allGalaxies = await _galaxyService.GetAllGalaxiesAsync();
                var inactiveGalaxies = allGalaxies.Where(g => !g.IsActive).ToList();

                if (!inactiveGalaxies.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No inactive galaxies found.[/]");
                    return;
                }

                ctx.Status("Select a galaxy to activate");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var allGalaxies = await _galaxyService.GetAllGalaxiesAsync();
        var inactiveGalaxies = allGalaxies.Where(g => !g.IsActive).ToList();

        if (!inactiveGalaxies.Any())
        {
            return;
        }

        var galaxyNames = inactiveGalaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
        var selectedGalaxyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a galaxy to activate")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(galaxyNames));

        int galaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Activating galaxy ID {galaxyId}...", async ctx =>
            {
                var result = await _galaxyService.ActivateGalaxyAsync(galaxyId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Galaxy activated successfully![/]");

                    var galaxy = await _galaxyService.GetGalaxyByIdAsync(galaxyId);
                    if (galaxy != null)
                    {
                        DisplayGalaxyDetails(galaxy);
                    }
                }
            });
    }

    private async Task DeactivateGalaxyAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to deactivate galaxies.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active galaxies...", async ctx =>
            {
                var activeGalaxies = await _galaxyService.GetActiveGalaxiesAsync();

                if (!activeGalaxies.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No active galaxies found.[/]");
                    return;
                }

                ctx.Status("Select a galaxy to deactivate");
                ctx.Spinner(Spinner.Known.Dots);
            });

        var activeGalaxies = await _galaxyService.GetActiveGalaxiesAsync();

        if (!activeGalaxies.Any())
        {
            return;
        }

        var galaxyNames = activeGalaxies.Select(g => $"{g.GalaxyId}: {g.GalaxyName}").ToList();
        var selectedGalaxyName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a galaxy to deactivate")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(galaxyNames));

        int galaxyId = int.Parse(selectedGalaxyName.Split(':')[0].Trim());

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Deactivating galaxy ID {galaxyId}...", async ctx =>
            {
                var result = await _galaxyService.DeactivateGalaxyAsync(galaxyId);

                if (result)
                {
                    AnsiConsole.MarkupLine("[green]Galaxy deactivated successfully![/]");

                    var galaxy = await _galaxyService.GetGalaxyByIdAsync(galaxyId);
                    if (galaxy != null)
                    {
                        DisplayGalaxyDetails(galaxy);
                    }
                }
            });
    }

    private void DisplayGalaxies(IEnumerable<Galaxy> galaxies, string title)
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

    private void DisplayGalaxyDetails(Galaxy galaxy)
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