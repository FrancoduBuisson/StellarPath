using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class CruiseCommandHandler : CommandHandlerBase<Cruise>
{
    private readonly CruiseService _cruiseService;
    private readonly SpaceshipService _spaceshipService;
    private readonly DestinationService _destinationService;

    public CruiseCommandHandler(CommandContext context, CruiseService cruiseService, SpaceshipService spaceshipService, DestinationService destinationService)
        : base(context)
    {
        _cruiseService = cruiseService;
        _spaceshipService = spaceshipService;
        _destinationService = destinationService;
    }

    protected override string GetMenuTitle() => "Cruise Management";
    protected override string GetEntityName() => "Cruise";
    protected override string GetEntityNamePlural() => "Cruises";

    protected override List<string> GetEntitySpecificOptions()
    {
        var options = new List<string>
        {
            "View Cruises by Spaceship",
            "View Cruises by Departure Destination",
            "View Cruises by Arrival Destination",
            "View Cruises by Date Range",
            "View My Created Cruises"
        };

        if (Context.CurrentUser?.Role == "Admin")
        {
            options.Add("Update Cruise Statuses");
        }

        return options;
    }

    protected override List<string> GetAdminOptions()
    {
        return new List<string>
        {
            $"Create New {GetEntityName()}",
            $"Cancel {GetEntityName()}"
        };
    }

    protected override async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "List All Cruises":
                await ListAllCruisesAsync();
                break;
            case "List Active Cruises":
                await ListActiveCruisesAsync();
                break;
            case "View Cruise Details":
                await ViewCruiseDetailsAsync();
                break;
            case "Search Cruises":
                await SearchCruisesAsync();
                break;
            case "View Cruises by Spaceship":
                await ViewCruisesBySpaceshipAsync();
                break;
            case "View Cruises by Departure Destination":
                await ViewCruisesByDepartureDestinationAsync();
                break;
            case "View Cruises by Arrival Destination":
                await ViewCruisesByArrivalDestinationAsync();
                break;
            case "View Cruises by Date Range":
                await ViewCruisesByDateRangeAsync();
                break;
            case "View My Created Cruises":
                await ViewMyCreatedCruisesAsync();
                break;
            case "Update Cruise Statuses":
                await UpdateCruiseStatusesAsync();
                break;
            case "Create New Cruise":
                await CreateCruiseAsync();
                break;
            case "Cancel Cruise":
                await CancelCruiseAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ListAllCruisesAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching cruises...", async ctx =>
        {
            var cruises = await _cruiseService.GetAllAsync();
            DisplayEntities(cruises, "All Cruises");
            return true;
        });
    }

    private async Task ListActiveCruisesAsync()
    {
        await ExecuteWithSpinnerAsync("Fetching active cruises...", async ctx =>
        {
            var scheduledCruises = await _cruiseService.GetCruisesByStatusIdAsync(1);
            var inProgressCruises = await _cruiseService.GetCruisesByStatusIdAsync(2);

            var activeCruises = scheduledCruises.Concat(inProgressCruises).ToList();
            DisplayEntities(activeCruises, "Active Cruises (Scheduled and In Progress)");
            return true;
        });
    }

    private async Task ViewCruiseDetailsAsync()
    {
        var cruise = await FetchAndPromptForEntitySelectionAsync<CruiseService, Cruise>(
            _cruiseService,
            service => service.GetAllAsync(),
            c => $"{c.DepartureDestinationName} to {c.ArrivalDestinationName} ({DisplayHelper.FormatDateTime(c.LocalDepartureTime)})",
            c => c.CruiseId,
            "Fetching cruises for selection...",
            "No cruises found.",
            "Select a cruise to view details");

        if (cruise != null)
        {
            await ExecuteWithSpinnerAsync($"Fetching details for cruise ID {cruise.CruiseId}...", async ctx =>
            {
                var fetchedCruise = await _cruiseService.GetByIdAsync(cruise.CruiseId);
                if (fetchedCruise != null)
                {
                    DisplayEntityDetails(fetchedCruise);
                }
                return true;
            });
        }
    }

    private async Task SearchCruisesAsync()
    {
        var searchCriteria = new CruiseSearchCriteria();

        var includeSpaceship = InputHelper.AskForConfirmation("Do you want to filter by spaceship?", false);
        if (includeSpaceship)
        {
            var spaceships = await ExecuteWithSpinnerAsync(
                "Fetching spaceships...",
                async ctx => await _spaceshipService.GetAllAsync());

            if (!spaceships.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No spaceships found for filtering.[/]");
            }
            else
            {
                var selectedSpaceship = SelectionHelper.SelectFromListById(
                    spaceships,
                    s => s.SpaceshipId,
                    s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
                    "Select a spaceship to filter by");

                if (selectedSpaceship != null)
                {
                    searchCriteria.SpaceshipId = selectedSpaceship.SpaceshipId;
                }
            }
        }

        var includeDeparture = InputHelper.AskForConfirmation("Do you want to filter by departure destination?", false);
        if (includeDeparture)
        {
            var destinations = await ExecuteWithSpinnerAsync(
                "Fetching destinations...",
                async ctx => await _destinationService.GetAllAsync());

            if (!destinations.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No destinations found for filtering.[/]");
            }
            else
            {
                var selectedDestination = SelectionHelper.SelectFromListById(
                    destinations,
                    d => d.DestinationId,
                    d => d.Name,
                    "Select a departure destination to filter by");

                if (selectedDestination != null)
                {
                    searchCriteria.DepartureDestinationId = selectedDestination.DestinationId;
                }
            }
        }

        var includeArrival = InputHelper.AskForConfirmation("Do you want to filter by arrival destination?", false);
        if (includeArrival)
        {
            var destinations = await ExecuteWithSpinnerAsync(
                "Fetching destinations...",
                async ctx => await _destinationService.GetAllAsync());

            if (!destinations.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No destinations found for filtering.[/]");
            }
            else
            {
                var selectedDestination = SelectionHelper.SelectFromListById(
                    destinations,
                    d => d.DestinationId,
                    d => d.Name,
                    "Select an arrival destination to filter by");

                if (selectedDestination != null)
                {
                    searchCriteria.ArrivalDestinationId = selectedDestination.DestinationId;
                }
            }
        }

        var includeDateRange = InputHelper.AskForConfirmation("Do you want to filter by date range?", false);
        if (includeDateRange)
        {
            searchCriteria.StartDate = InputHelper.AskForDateTime("Enter start date and time (YYYY-MM-DD HH:MM):", DateTime.Now);
            searchCriteria.EndDate = InputHelper.AskForDateTime("Enter end date and time (YYYY-MM-DD HH:MM):", searchCriteria.StartDate?.AddDays(7));

            if (searchCriteria.StartDate >= searchCriteria.EndDate)
            {
                AnsiConsole.MarkupLine("[yellow]Start date must be before end date. Using default end date (start date + 7 days).[/]");
                searchCriteria.EndDate = searchCriteria.StartDate?.AddDays(7);
            }
        }

        var includeStatus = InputHelper.AskForConfirmation("Do you want to filter by cruise status?", false);
        if (includeStatus)
        {
            var statuses = await ExecuteWithSpinnerAsync(
                "Fetching cruise statuses...",
                async ctx => await _cruiseService.GetAllCruiseStatusesAsync());

            var selectedStatus = SelectionHelper.SelectFromListById(
                statuses,
                s => s.CruiseStatusId,
                s => s.StatusName,
                "Select a status to filter by");

            if (selectedStatus != null)
            {
                searchCriteria.StatusId = selectedStatus.CruiseStatusId;
                searchCriteria.StatusName = selectedStatus.StatusName;
            }
        }

        var includePrice = InputHelper.AskForConfirmation("Do you want to filter by price range?", false);
        if (includePrice)
        {
            var includeMinPrice = InputHelper.AskForConfirmation("Set minimum price?", false);
            if (includeMinPrice)
            {
                searchCriteria.MinPrice = InputHelper.AskForDecimal("Enter minimum price:", min: 0);
            }

            var includeMaxPrice = InputHelper.AskForConfirmation("Set maximum price?", false);
            if (includeMaxPrice)
            {
                searchCriteria.MaxPrice = InputHelper.AskForDecimal("Enter maximum price:", min: searchCriteria.MinPrice ?? 0);
            }
        }

        await ExecuteWithSpinnerAsync("Searching cruises...", async ctx =>
        {
            var cruises = await _cruiseService.SearchCruisesAsync(searchCriteria);

            string title = BuildSearchResultTitle(searchCriteria);
            DisplayEntities(cruises, title);
            return true;
        });
    }

    private string BuildSearchResultTitle(CruiseSearchCriteria criteria)
    {
        var titleParts = new List<string> { "Search Results for Cruises" };

        if (criteria.SpaceshipId.HasValue)
        {
            titleParts.Add($"with spaceship ID {criteria.SpaceshipId.Value}");
        }

        if (criteria.DepartureDestinationId.HasValue)
        {
            titleParts.Add($"departing from destination ID {criteria.DepartureDestinationId.Value}");
        }

        if (criteria.ArrivalDestinationId.HasValue)
        {
            titleParts.Add($"arriving at destination ID {criteria.ArrivalDestinationId.Value}");
        }

        if (criteria.StartDate.HasValue)
        {
            titleParts.Add($"starting from {DisplayHelper.FormatDateTime(criteria.StartDate.Value)}");
        }

        if (criteria.EndDate.HasValue)
        {
            titleParts.Add($"ending at {DisplayHelper.FormatDateTime(criteria.EndDate.Value)}");
        }

        if (criteria.StatusId.HasValue && !string.IsNullOrEmpty(criteria.StatusName))
        {
            titleParts.Add($"with status '{criteria.StatusName}'");
        }

        if (criteria.MinPrice.HasValue)
        {
            titleParts.Add($"with price >= {DisplayHelper.FormatPrice(criteria.MinPrice.Value)}");
        }

        if (criteria.MaxPrice.HasValue)
        {
            titleParts.Add($"with price <= {DisplayHelper.FormatPrice(criteria.MaxPrice.Value)}");
        }

        return string.Join(" ", titleParts);
    }

    private async Task ViewCruisesBySpaceshipAsync()
    {
        var spaceships = await ExecuteWithSpinnerAsync(
            "Fetching spaceships...",
            async ctx => await _spaceshipService.GetAllAsync());

        if (!spaceships.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No spaceships found.[/]");
            return;
        }

        var selectedSpaceship = SelectionHelper.SelectFromListById(
            spaceships,
            s => s.SpaceshipId,
            s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
            "Select a spaceship to view its cruises");

        if (selectedSpaceship == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching cruises for spaceship ID {selectedSpaceship.SpaceshipId}...", async ctx =>
        {
            var cruises = await _cruiseService.GetCruisesBySpaceshipIdAsync(selectedSpaceship.SpaceshipId);
            DisplayEntities(cruises, $"Cruises for Spaceship: {selectedSpaceship.ModelName} #{selectedSpaceship.SpaceshipId}");
            return true;
        });
    }

    private async Task ViewCruisesByDepartureDestinationAsync()
    {
        var destinations = await ExecuteWithSpinnerAsync(
            "Fetching destinations...",
            async ctx => await _destinationService.GetAllAsync());

        if (!destinations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No destinations found.[/]");
            return;
        }

        var selectedDestination = SelectionHelper.SelectFromListById(
            destinations,
            d => d.DestinationId,
            d => d.Name,
            "Select a departure destination to view its cruises");

        if (selectedDestination == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching cruises from departure destination ID {selectedDestination.DestinationId}...", async ctx =>
        {
            var cruises = await _cruiseService.GetCruisesByDepartureDestinationAsync(selectedDestination.DestinationId);
            DisplayEntities(cruises, $"Cruises Departing from: {selectedDestination.Name}");
            return true;
        });
    }

    private async Task ViewCruisesByArrivalDestinationAsync()
    {
        var destinations = await ExecuteWithSpinnerAsync(
            "Fetching destinations...",
            async ctx => await _destinationService.GetAllAsync());

        if (!destinations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No destinations found.[/]");
            return;
        }

        var selectedDestination = SelectionHelper.SelectFromListById(
            destinations,
            d => d.DestinationId,
            d => d.Name,
            "Select an arrival destination to view its cruises");

        if (selectedDestination == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching cruises to arrival destination ID {selectedDestination.DestinationId}...", async ctx =>
        {
            var cruises = await _cruiseService.GetCruisesByArrivalDestinationAsync(selectedDestination.DestinationId);
            DisplayEntities(cruises, $"Cruises Arriving at: {selectedDestination.Name}");
            return true;
        });
    }

    private async Task ViewCruisesByDateRangeAsync()
    {
        var startDate = InputHelper.AskForDateTime("Enter start date and time (YYYY-MM-DD HH:MM):", DateTime.Now);
        var endDate = InputHelper.AskForDateTime("Enter end date and time (YYYY-MM-DD HH:MM):", startDate.AddDays(7));

        if (startDate >= endDate)
        {
            AnsiConsole.MarkupLine("[yellow]Start date must be before end date. Please try again.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching cruises between {DisplayHelper.FormatDateTime(startDate)} and {DisplayHelper.FormatDateTime(endDate)}...", async ctx =>
        {
            var cruises = await _cruiseService.GetCruisesBetweenDatesAsync(startDate, endDate);
            DisplayEntities(cruises, $"Cruises between {DisplayHelper.FormatDateTime(startDate)} and {DisplayHelper.FormatDateTime(endDate)}");
            return true;
        });
    }

    private async Task ViewMyCreatedCruisesAsync()
    {
        if (Context.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to view your created cruises.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync("Fetching your created cruises...", async ctx =>
        {
            var cruises = await _cruiseService.GetMyCruisesAsync();
            DisplayEntities(cruises, "Your Created Cruises");
            return true;
        });
    }

    private async Task UpdateCruiseStatusesAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        await ExecuteWithSpinnerAsync("Updating cruise statuses...", async ctx =>
        {
            var result = await _cruiseService.UpdateCruiseStatusesAsync();
            if (result)
            {
                AnsiConsole.MarkupLine("[green]Cruise statuses updated successfully![/]");
            }
            return result;
        });
    }

    private async Task CreateCruiseAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var spaceships = await ExecuteWithSpinnerAsync(
            "Fetching active spaceships...",
            async ctx => await _spaceshipService.GetActiveAsync());

        if (!spaceships.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active spaceships found. Please create a spaceship first.[/]");
            return;
        }

        var selectedSpaceship = SelectionHelper.SelectFromListById(
            spaceships,
            s => s.SpaceshipId,
            s => s.ModelName != null ? $"{s.ModelName} #{s.SpaceshipId}" : $"Spaceship #{s.SpaceshipId}",
            "Select a spaceship for this cruise");

        if (selectedSpaceship == null)
        {
            return;
        }

        var destinations = await ExecuteWithSpinnerAsync(
            "Fetching active destinations...",
            async ctx => await _destinationService.GetActiveAsync());

        if (!destinations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active destinations found. Please create a destination first.[/]");
            return;
        }

        var departureDestination = SelectionHelper.SelectFromListById(
            destinations,
            d => d.DestinationId,
            d => d.Name,
            "Select the departure destination");

        if (departureDestination == null)
        {
            return;
        }

        var arrivalDestinations = destinations.Where(d => d.DestinationId != departureDestination.DestinationId).ToList();
        if (!arrivalDestinations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No other destinations available for arrival. Please create another destination.[/]");
            return;
        }

        var arrivalDestination = SelectionHelper.SelectFromListById(
            arrivalDestinations,
            d => d.DestinationId,
            d => d.Name,
            "Select the arrival destination");

        if (arrivalDestination == null)
        {
            return;
        }

        var departureTime = InputHelper.AskForDateTime("Enter departure date and time (YYYY-MM-DD HH:MM):", DateTime.Now.AddDays(1));

        if (departureTime < DateTime.Now)
        {
            AnsiConsole.MarkupLine("[yellow]Departure time must be in the future. Using tomorrow's date instead.[/]");
            departureTime = DateTime.Now.AddDays(1);
        }

        var price = InputHelper.AskForDecimal("Enter seat price ($):", min: 1.0m);

        var newCruise = new CreateCruiseDto
        {
            SpaceshipId = selectedSpaceship.SpaceshipId,
            DepartureDestinationId = departureDestination.DestinationId,
            ArrivalDestinationId = arrivalDestination.DestinationId,
            LocalDepartureTime = departureTime,
            CruiseSeatPrice = price
        };

        await ExecuteWithSpinnerAsync("Creating cruise...", async ctx =>
        {
            var result = await _cruiseService.CreateCruiseAsync(newCruise);

            if (result.HasValue)
            {
                AnsiConsole.MarkupLine($"[green]Cruise created successfully with ID: {result.Value}[/]");

                var cruise = await _cruiseService.GetByIdAsync(result.Value);
                if (cruise != null)
                {
                    DisplayEntityDetails(cruise);
                }
            }
            return true;
        });
    }

    private async Task CancelCruiseAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var scheduledCruises = await ExecuteWithSpinnerAsync(
            "Fetching scheduled cruises...",
            async ctx => await _cruiseService.GetCruisesByStatusIdAsync(1));

        if (!scheduledCruises.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No scheduled cruises found that can be cancelled.[/]");
            return;
        }

        var selectedCruise = SelectionHelper.SelectFromListById(
            scheduledCruises,
            c => c.CruiseId,
            c => $"{c.DepartureDestinationName} to {c.ArrivalDestinationName} ({DisplayHelper.FormatDateTime(c.LocalDepartureTime)})",
            "Select a cruise to cancel");

        if (selectedCruise == null)
        {
            return;
        }

        var confirm = InputHelper.AskForConfirmation($"Are you sure you want to cancel cruise #{selectedCruise.CruiseId}?", false);
        if (!confirm)
        {
            AnsiConsole.MarkupLine("[grey]Cancellation aborted.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync($"Cancelling cruise ID {selectedCruise.CruiseId}...", async ctx =>
        {
            var result = await _cruiseService.CancelCruiseAsync(selectedCruise.CruiseId);

            if (result)
            {
                AnsiConsole.MarkupLine("[green]Cruise cancelled successfully![/]");

                var cruise = await _cruiseService.GetByIdAsync(selectedCruise.CruiseId);
                if (cruise != null)
                {
                    DisplayEntityDetails(cruise);
                }
            }
            return true;
        });
    }

    protected override void DisplayEntities(IEnumerable<Cruise> cruises, string title)
    {
        var cruiseList = cruises.ToList();

        var columns = new[]
        {
            "ID",
            "Route",
            "Departure Time",
            "Duration",
            "Spaceship",
            "Price",
            "Status"
        };

        var rows = cruiseList.Select(c => new[]
        {
            c.CruiseId.ToString(),
            $"{c.DepartureDestinationName} -> {c.ArrivalDestinationName}",
            $"{DisplayHelper.FormatDateTime(c.LocalDepartureTime)}",
            FormatDuration(c.DurationMinutes),
            $"{c.SpaceshipName ?? "Unknown"} (ID: {c.SpaceshipId})",
            $"{DisplayHelper.FormatPrice(c.CruiseSeatPrice)}",
            c.CruiseStatusName ?? $"Status ID: {c.CruiseStatusId}"
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(Cruise cruise)
    {
        var estimatedArrival = cruise.LocalDepartureTime.AddMinutes(cruise.DurationMinutes);

        var details = new Dictionary<string, string>
        {
            ["Cruise ID"] = cruise.CruiseId.ToString(),
            ["Spaceship"] = $"{cruise.SpaceshipName ?? "Unknown"} (ID: {cruise.SpaceshipId})",
            ["Capacity"] = cruise.Capacity?.ToString() ?? "Unknown",
            ["Cruise Speed"] = cruise.CruiseSpeedKmph.HasValue ? $"{cruise.CruiseSpeedKmph.Value:N0} kmph" : "Unknown",
            ["Route"] = $"{cruise.DepartureDestinationName} -> {cruise.ArrivalDestinationName}",
            ["Departure Time"] = $"{DisplayHelper.FormatDateTime(cruise.LocalDepartureTime)}",
            ["Estimated Arrival"] = $"{DisplayHelper.FormatDateTime(estimatedArrival)}",
            ["Duration"] = FormatDuration(cruise.DurationMinutes),
            ["Seat Price"] = $"{DisplayHelper.FormatPrice(cruise.CruiseSeatPrice)}",
            ["Status"] = cruise.CruiseStatusName ?? $"Status ID: {cruise.CruiseStatusId}",
            ["Created By"] = cruise.CreatedByName ?? cruise.CreatedByGoogleId
        };

        DisplayHelper.DisplayDetails("Cruise Details", details);
    }

    private string FormatDuration(int minutes)
    {
        var timeSpan = TimeSpan.FromMinutes(minutes);

        if (timeSpan.TotalHours < 1)
        {
            return $"{timeSpan.Minutes} minutes";
        }

        if (timeSpan.TotalDays < 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }

        return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
    }
}