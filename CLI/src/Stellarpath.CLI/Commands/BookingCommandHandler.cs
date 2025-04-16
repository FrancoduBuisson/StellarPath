using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Commands;

public class BookingCommandHandler : CommandHandlerBase<Booking>
{
    private readonly BookingService _bookingService;
    private readonly CruiseService _cruiseService;
    private readonly UserService _userService;

    public BookingCommandHandler(CommandContext context, BookingService bookingService, CruiseService cruiseService, UserService userService)
        : base(context)
    {
        _bookingService = bookingService;
        _cruiseService = cruiseService;
        _userService = userService;
    }

    protected override string GetMenuTitle() => "Booking Management";
    protected override string GetEntityName() => "Booking";
    protected override string GetEntityNamePlural() => "Bookings";

    protected override List<string> GetBaseOptions()
    {
        var options = new List<string>
        {
            "View My Bookings",
            "View Booking Details",
            "Book a Cruise"
        };

        if (Context.CurrentUser?.Role == "Admin")
        {
            options.Add("Search Bookings");
            options.Add("View Bookings by Cruise");
            options.Add("View Booking History");
        }

        return options;
    }

    protected override List<string> GetEntitySpecificOptions()
    {
        return new List<string>
        {
            "Pay for Booking",
            "Cancel Booking"
        };
    }

    protected override List<string> GetAdminOptions()
    {
        return new List<string>();
    }

    protected override async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "View My Bookings":
                await ViewMyBookingsAsync();
                break;
            case "View Booking Details":
                await ViewBookingDetailsAsync();
                break;
            case "Book a Cruise":
                await BookCruiseAsync();
                break;
            case "Search Bookings":
                await SearchBookingsAsync();
                break;
            case "View Bookings by Cruise":
                await ViewBookingsByCruiseAsync();
                break;
            case "View Booking History":
                await ViewBookingHistoryAsync();
                break;
            case "Pay for Booking":
                await PayForBookingAsync();
                break;
            case "Cancel Booking":
                await CancelBookingAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ViewMyBookingsAsync()
    {
        if (Context.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to view your bookings.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync("Fetching your bookings...", async ctx =>
        {
            var bookings = await _bookingService.GetMyBookingsAsync();
            if (!bookings.Any())
            {
                AnsiConsole.MarkupLine("[yellow]You don't have any bookings.[/]");
                return true;
            }

            DisplayEntities(bookings, "Your Bookings");
            return true;
        });
    }

    private async Task ViewBookingDetailsAsync()
    {
        if (Context.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to view booking details.[/]");
            return;
        }

        var bookings = await ExecuteWithSpinnerAsync("Fetching your bookings...", async ctx =>
        {
            return await _bookingService.GetMyBookingsAsync();
        });

        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You don't have any bookings to view.[/]");
            return;
        }

        var selectedBooking = SelectionHelper.SelectFromListById(
            bookings,
            b => b.BookingId,
            b => $"{b.DepartureDestination} to {b.ArrivalDestination} (Seat {b.SeatNumber})",
            "Select a booking to view details");

        if (selectedBooking == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching details for booking ID {selectedBooking.BookingId}...", async ctx =>
        {
            var booking = await _bookingService.GetByIdAsync(selectedBooking.BookingId);
            if (booking != null)
            {
                DisplayEntityDetails(booking);
            }
            return true;
        });
    }

    private async Task BookCruiseAsync()
    {
        if (Context.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to book a cruise.[/]");
            return;
        }

        var cruises = await ExecuteWithSpinnerAsync(
            "Fetching available cruises...",
            async ctx =>
            {
                var scheduled = await _cruiseService.GetCruisesByStatusIdAsync(1);
                return scheduled.ToList();
            });

        if (!cruises.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No available cruises found to book.[/]");
            return;
        }

        var rows = cruises.Select(c => new[]
        {
            c.CruiseId.ToString(),
            $"{c.DepartureDestinationName} -> {c.ArrivalDestinationName}",
            DisplayHelper.FormatDateTime(c.LocalDepartureTime),
            FormatDuration(c.DurationMinutes),
            $"{c.SpaceshipName ?? "Unknown"} (Capacity: {c.Capacity})",
            DisplayHelper.FormatPrice(c.CruiseSeatPrice),
            c.CruiseStatusName ?? "Unknown"
        });

        DisplayHelper.DisplayTable(
            "Available Cruises",
            new[] { "ID", "Route", "Departure Time", "Duration", "Spaceship", "Price", "Status" },
            rows);

        var selectedCruise = SelectionHelper.SelectFromListById(
            cruises,
            c => c.CruiseId,
            c => $"{c.DepartureDestinationName} to {c.ArrivalDestinationName} ({DisplayHelper.FormatDateTime(c.LocalDepartureTime)})",
            "Select a cruise to book");

        if (selectedCruise == null)
        {
            return;
        }

        var availableSeats = await ExecuteWithSpinnerAsync(
            $"Fetching available seats for cruise {selectedCruise.CruiseId}...",
            async ctx => await _bookingService.GetAvailableSeatsForCruiseAsync(selectedCruise.CruiseId));

        if (!availableSeats.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No available seats on this cruise.[/]");
            return;
        }

        await DisplayVisualSeatMap(selectedCruise.CruiseId, selectedCruise.Capacity ?? 0);

        AnsiConsole.MarkupLine($"Price per seat: [cyan]{selectedCruise.CruiseSeatPrice:C2}[/]");
        var confirmBooking = InputHelper.AskForConfirmation("Do you want to proceed with booking?", true);
        if (!confirmBooking)
        {
            AnsiConsole.MarkupLine("[grey]Booking cancelled.[/]");
            return;
        }

        var availableSeatsArray = availableSeats.ToArray();
        var seatOptions = availableSeatsArray.Select(s => $"Seat {s}").ToList();

        var selectedSeatOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a seat")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(seatOptions));

        int seatNumber = int.Parse(selectedSeatOption.Replace("Seat ", ""));

        var createBookingDto = new CreateBookingDto
        {
            CruiseId = selectedCruise.CruiseId,
            SeatNumber = seatNumber
        };

        int? bookingId = await ExecuteWithSpinnerAsync("Creating your booking...", async ctx =>
        {
            return await _bookingService.CreateBookingAsync(createBookingDto);
        });

        if (bookingId.HasValue)
        {
            AnsiConsole.MarkupLine($"[green]Booking created successfully with ID: {bookingId.Value}[/]");
            AnsiConsole.MarkupLine("[yellow]Note: Your reservation will expire in 30 minutes if not paid.[/]");

            var booking = await ExecuteWithSpinnerAsync("Fetching booking details...", async ctx =>
                await _bookingService.GetByIdAsync(bookingId.Value));

            if (booking != null)
            {
                DisplayEntityDetails(booking);
            }

            var payNow = InputHelper.AskForConfirmation("Do you want to pay for this booking now?", true);
            if (payNow)
            {
                var paymentSuccess = await ExecuteWithSpinnerAsync($"Processing payment for booking ID {bookingId.Value}...", async ctx =>
                    await _bookingService.PayForBookingAsync(bookingId.Value));

                if (paymentSuccess)
                {
                    AnsiConsole.MarkupLine("[green]Payment successful! Your booking is confirmed.[/]");

                    var updatedBooking = await ExecuteWithSpinnerAsync("Fetching updated booking details...", async ctx =>
                        await _bookingService.GetByIdAsync(bookingId.Value));

                    if (updatedBooking != null)
                    {
                        DisplayEntityDetails(updatedBooking);
                    }
                }
            }
        }
    }

    private async Task SearchBookingsAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var searchCriteria = new SearchBookingsDto();

        var criteriaOptions = new List<string>
        {
            "User (Google ID)",
            "Cruise ID",
            "Booking Status",
            "Date Range",
            "Seat Number"
        };

        var selectedCriteria = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[yellow]Select search criteria to include[/]")
                .PageSize(10)
                .InstructionsText("[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
                .AddChoices(criteriaOptions));

        if (selectedCriteria.Contains("User (Google ID)"))
        {
            var users = await ExecuteWithSpinnerAsync(
                "Fetching users...",
                async ctx => await _userService.GetAllUsersAsync());

            if (users.Any())
            {
                var selectedUser = SelectionHelper.SelectFromList(
                    users,
                    u => $"{u.FirstName} {u.LastName} ({u.Email})",
                    "Select a user to filter by");

                if (selectedUser != null)
                {
                    searchCriteria.GoogleId = selectedUser.GoogleId;
                }
            }
        }

        if (selectedCriteria.Contains("Cruise ID"))
        {
            searchCriteria.CruiseId = InputHelper.AskForInt("[cyan]Enter cruise ID:[/]", min: 1);
        }

        if (selectedCriteria.Contains("Booking Status"))
        {
            var statusOptions = new[] { "Reserved", "Paid", "Completed", "Cancelled", "Expired" };
            searchCriteria.StatusName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Select booking status[/]")
                    .PageSize(10)
                    .AddChoices(statusOptions));
        }

        if (selectedCriteria.Contains("Date Range"))
        {
            var includeDateFrom = InputHelper.AskForConfirmation("Set from date?", true);
            if (includeDateFrom)
            {
                searchCriteria.FromDate = InputHelper.AskForDate("[cyan]Enter from date (YYYY-MM-DD):[/]");
            }

            var includeDateTo = InputHelper.AskForConfirmation("Set to date?", true);
            if (includeDateTo)
            {
                searchCriteria.ToDate = InputHelper.AskForDate("[cyan]Enter to date (YYYY-MM-DD):[/]",
                    searchCriteria.FromDate?.AddDays(30));
            }
        }

        if (selectedCriteria.Contains("Seat Number"))
        {
            searchCriteria.SeatNumber = InputHelper.AskForInt("[cyan]Enter seat number:[/]", min: 1);
        }

        await ExecuteWithSpinnerAsync("Searching bookings...", async ctx =>
        {
            var bookings = await _bookingService.SearchBookingsAsync(searchCriteria);

            var title = "Search Results for Bookings";
            if (!string.IsNullOrEmpty(searchCriteria.GoogleId))
                title += $" by user ID {searchCriteria.GoogleId}";
            if (searchCriteria.CruiseId.HasValue)
                title += $" for cruise ID {searchCriteria.CruiseId.Value}";
            if (!string.IsNullOrEmpty(searchCriteria.StatusName))
                title += $" with status '{searchCriteria.StatusName}'";
            if (searchCriteria.FromDate.HasValue)
                title += $" from {searchCriteria.FromDate.Value.ToShortDateString()}";
            if (searchCriteria.ToDate.HasValue)
                title += $" to {searchCriteria.ToDate.Value.ToShortDateString()}";
            if (searchCriteria.SeatNumber.HasValue)
                title += $" for seat number {searchCriteria.SeatNumber.Value}";

            DisplayEntities(bookings, title);
            return true;
        });
    }

    private async Task ViewBookingsByCruiseAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var cruises = await ExecuteWithSpinnerAsync(
            "Fetching cruises...",
            async ctx => await _cruiseService.GetAllAsync());

        if (!cruises.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No cruises found.[/]");
            return;
        }

        var selectedCruise = SelectionHelper.SelectFromListById(
            cruises,
            c => c.CruiseId,
            c => $"{c.DepartureDestinationName} to {c.ArrivalDestinationName} ({DisplayHelper.FormatDateTime(c.LocalDepartureTime)})",
            "Select a cruise to view its bookings");

        if (selectedCruise == null)
        {
            return;
        }

        await ExecuteWithSpinnerAsync($"Fetching bookings for cruise ID {selectedCruise.CruiseId}...", async ctx =>
        {
            var bookings = await _bookingService.GetBookingsByCruiseAsync(selectedCruise.CruiseId);
            DisplayEntities(bookings, $"Bookings for Cruise: {selectedCruise.DepartureDestinationName} to {selectedCruise.ArrivalDestinationName}");
            return true;
        });
    }

    private async Task ViewBookingHistoryAsync()
    {
        if (!EnsureAdminPermission())
        {
            return;
        }

        var searchCriteria = new SearchBookingHistoryDto();

        var criteriaOptions = new List<string>
        {
            "Booking ID",
            "Status Change",
            "Date Range"
        };

        var selectedCriteria = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[yellow]Select search criteria to include[/]")
                .PageSize(10)
                .InstructionsText("[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
                .AddChoices(criteriaOptions));

        if (selectedCriteria.Contains("Booking ID"))
        {
            searchCriteria.BookingId = InputHelper.AskForInt("[cyan]Enter booking ID:[/]", min: 1);
        }

        if (selectedCriteria.Contains("Status Change"))
        {
            var includeFrom = InputHelper.AskForConfirmation("Filter by previous status?", false);
            if (includeFrom)
            {
                var statusOptions = new[] { "Reserved", "Paid", "Completed", "Cancelled", "Expired" };
                searchCriteria.PreviousStatusName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[cyan]Select previous status[/]")
                        .PageSize(10)
                        .AddChoices(statusOptions));
            }

            var includeTo = InputHelper.AskForConfirmation("Filter by new status?", false);
            if (includeTo)
            {
                var statusOptions = new[] { "Reserved", "Paid", "Completed", "Cancelled", "Expired" };
                searchCriteria.NewStatusName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[cyan]Select new status[/]")
                        .PageSize(10)
                        .AddChoices(statusOptions));
            }
        }

        if (selectedCriteria.Contains("Date Range"))
        {
            var includeDateFrom = InputHelper.AskForConfirmation("Set from date?", true);
            if (includeDateFrom)
            {
                searchCriteria.FromDate = InputHelper.AskForDate("[cyan]Enter from date (YYYY-MM-DD):[/]");
            }

            var includeDateTo = InputHelper.AskForConfirmation("Set to date?", true);
            if (includeDateTo)
            {
                searchCriteria.ToDate = InputHelper.AskForDate("[cyan]Enter to date (YYYY-MM-DD):[/]",
                    searchCriteria.FromDate?.AddDays(30));
            }
        }

        await ExecuteWithSpinnerAsync("Searching booking history...", async ctx =>
        {
            var history = await _bookingService.SearchBookingHistoryAsync(searchCriteria);

            if (!history.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No booking history found matching your criteria.[/]");
                return true;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.DarkCyan)
                .Title("[bold blue]Booking History Results[/]")
                .Expand();

            table.AddColumn(new TableColumn("[bold]History ID[/]"));
            table.AddColumn(new TableColumn("[bold]Booking ID[/]"));
            table.AddColumn(new TableColumn("[bold]From Status[/]"));
            table.AddColumn(new TableColumn("[bold]To Status[/]"));
            table.AddColumn(new TableColumn("[bold]Changed At[/]"));

            foreach (var item in history)
            {
                table.AddRow(
                    $"[grey]{item.HistoryId}[/]",
                    item.BookingId.ToString(),
                    FormatStatus(item.PreviousStatus),
                    FormatStatus(item.NewStatus),
                    DisplayHelper.FormatDateTime(item.ChangedAt)
                );
            }

            AnsiConsole.Write(table);
            return true;
        });
    }

    private async Task PayForBookingAsync()
    {
        if (Context.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to pay for a booking.[/]");
            return;
        }

        var bookings = await ExecuteWithSpinnerAsync(
            "Fetching your unpaid bookings...",
            async ctx =>
            {
                var allBookings = await _bookingService.GetMyBookingsAsync();
                return allBookings.Where(b => b.BookingStatusName == "Reserved").ToList();
            });

        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You don't have any unpaid bookings.[/]");
            return;
        }

        var selectedBooking = SelectionHelper.SelectFromListById(
            bookings,
            b => b.BookingId,
            b => $"{b.DepartureDestination} to {b.ArrivalDestination} (Seat {b.SeatNumber})",
            "Select a booking to pay for");

        if (selectedBooking == null)
        {
            return;
        }

        var confirmPayment = InputHelper.AskForConfirmation($"Confirm payment for booking #{selectedBooking.BookingId}?", true);
        if (!confirmPayment)
        {
            AnsiConsole.MarkupLine("[grey]Payment cancelled.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync($"Processing payment for booking ID {selectedBooking.BookingId}...", async ctx =>
        {
            var success = await _bookingService.PayForBookingAsync(selectedBooking.BookingId);
            if (success)
            {
                AnsiConsole.MarkupLine("[green]Payment processed successfully![/]");

                var updatedBooking = await _bookingService.GetByIdAsync(selectedBooking.BookingId);
                if (updatedBooking != null)
                {
                    DisplayEntityDetails(updatedBooking);
                }
            }
            return true;
        });
    }

    private async Task CancelBookingAsync()
    {
        if (Context.CurrentUser == null)
        {
            AnsiConsole.MarkupLine("[red]You must be logged in to cancel a booking.[/]");
            return;
        }

        var bookings = await ExecuteWithSpinnerAsync(
            "Fetching your active bookings...",
            async ctx =>
            {
                var allBookings = await _bookingService.GetMyBookingsAsync();
                return allBookings.Where(b =>
                    b.BookingStatusName == "Reserved" ||
                    b.BookingStatusName == "Paid").ToList();
            });

        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You don't have any active bookings to cancel.[/]");
            return;
        }

        var selectedBooking = SelectionHelper.SelectFromListById(
            bookings,
            b => b.BookingId,
            b => $"{b.DepartureDestination} to {b.ArrivalDestination} (Seat {b.SeatNumber} - {b.BookingStatusName})",
            "Select a booking to cancel");

        if (selectedBooking == null)
        {
            return;
        }

        var confirmCancellation = InputHelper.AskForConfirmation($"Are you sure you want to cancel booking #{selectedBooking.BookingId}?", false);
        if (!confirmCancellation)
        {
            AnsiConsole.MarkupLine("[grey]Cancellation aborted.[/]");
            return;
        }

        await ExecuteWithSpinnerAsync($"Cancelling booking ID {selectedBooking.BookingId}...", async ctx =>
        {
            var success = await _bookingService.CancelBookingAsync(selectedBooking.BookingId);
            if (success)
            {
                AnsiConsole.MarkupLine("[green]Booking cancelled successfully![/]");

                var updatedBooking = await _bookingService.GetByIdAsync(selectedBooking.BookingId);
                if (updatedBooking != null)
                {
                    DisplayEntityDetails(updatedBooking);
                }
            }
            return true;
        });
    }

    protected override void DisplayEntities(IEnumerable<Booking> bookings, string title)
    {
        var bookingList = bookings.ToList();

        var columns = new[]
        {
            "ID",
            "Route",
            "Departure",
            "Seat",
            "Booked On",
            "Status",
            "User"
        };

        var rows = bookingList.Select(b => new[]
        {
            b.BookingId.ToString(),
            $"{b.DepartureDestination} -> {b.ArrivalDestination}",
            DisplayHelper.FormatDateTime(b.LocalDepartureTime),
            b.SeatNumber.ToString(),
            DisplayHelper.FormatDateTime(b.BookingDate),
            FormatStatus(b.BookingStatusName),
            b.UserName
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    protected override void DisplayEntityDetails(Booking booking)
    {
        var details = new Dictionary<string, string>
        {
            ["Booking ID"] = booking.BookingId.ToString(),
            ["Route"] = $"{booking.DepartureDestination} -> {booking.ArrivalDestination}",
            ["Departure Time"] = DisplayHelper.FormatDateTime(booking.LocalDepartureTime),
            ["Seat Number"] = booking.SeatNumber.ToString(),
            ["Booking Date"] = DisplayHelper.FormatDateTime(booking.BookingDate),
            ["Status"] = FormatStatus(booking.BookingStatusName)
        };

        if (booking.BookingStatusName == "Reserved")
        {
            details["Expires On"] = DisplayHelper.FormatDateTime(booking.BookingExpiration);
        }

        if (Context.CurrentUser?.Role == "Admin" || booking.GoogleId == Context.CurrentUser?.GoogleId)
        {
            details["User"] = booking.UserName;
            details["User ID"] = booking.GoogleId;
        }

        DisplayHelper.DisplayDetails("Booking Details", details);
    }

    private string FormatStatus(string status)
    {
        return status switch
        {
            "Reserved" => "[yellow]Reserved[/]",
            "Paid" => "[green]Paid[/]",
            "Completed" => "[blue]Completed[/]",
            "Cancelled" => "[red]Cancelled[/]",
            "Expired" => "[grey]Expired[/]",
            _ => status
        };
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

    private async Task DisplayVisualSeatMap(int cruiseId, int capacity)
    {
        var availableSeats = await _bookingService.GetAvailableSeatsForCruiseAsync(cruiseId);
        var allSeats = Enumerable.Range(1, capacity).ToList();

        int columns = Math.Min(10, capacity);
        int rows = (int)Math.Ceiling(capacity / (double)columns);

        AnsiConsole.MarkupLine("[blue bold]Spaceship Seat Map[/]");
        AnsiConsole.MarkupLine("[grey]Legend: [green]■[/] Available  [red]□[/] Occupied[/]");

        var grid = new Grid();

        for (int i = 0; i < columns; i++)
        {
            grid.AddColumn(new GridColumn().NoWrap().PadRight(1));
        }

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            var rowCells = new Markup[columns];

            for (int colIndex = 0; colIndex < columns; colIndex++)
            {
                int seatNumber = rowIndex * columns + colIndex + 1;

                if (seatNumber <= capacity)
                {
                    bool isAvailable = availableSeats.Contains(seatNumber);
                    rowCells[colIndex] = new Markup(
                        isAvailable
                            ? $"[green]■[/] [green]{seatNumber,2}[/]"
                            : $"[red]□[/] [grey]{seatNumber,2}[/]");
                }
                else
                {
                    rowCells[colIndex] = new Markup(" ");
                }
            }

            grid.AddRow(rowCells);
        }

        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();
    }

}