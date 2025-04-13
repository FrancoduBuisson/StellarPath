using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;
using System.Text;

namespace Stellarpath.CLI.Commands;

public class UserCommandHandler
{
    private readonly CommandContext _context;
    private readonly UserService _userService;
    private readonly List<string> _availableRoles = new() { "User", "Admin" };

    public UserCommandHandler(CommandContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task HandleAsync()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            await ViewCurrentUserAsync();
            return;
        }

        var options = new List<string>
        {
            "View My User Info",
            "List All Users",
            "View User Details",
            "Search Users",
            "Activate User",
            "Deactivate User",
            "Update User Role",
            "Back to Main Menu"
        };

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("User Management")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        await ProcessSelectionAsync(selection);
    }

    private async Task ProcessSelectionAsync(string selection)
    {
        switch (selection)
        {
            case "View My User Info":
                await ViewCurrentUserAsync();
                break;
            case "List All Users":
                await ListAllUsersAsync();
                break;
            case "View User Details":
                await ViewUserDetailsAsync();
                break;
            case "Search Users":
                await SearchUsersAsync();
                break;
            case "Activate User":
                await ActivateUserAsync();
                break;
            case "Deactivate User":
                await DeactivateUserAsync();
                break;
            case "Update User Role":
                await UpdateUserRoleAsync();
                break;
            case "Back to Main Menu":
                return;
        }
    }

    private async Task ViewCurrentUserAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching your user information...", async ctx =>
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    DisplayUserDetails(user, "Your User Profile");
                }
            });
    }

    private async Task ListAllUsersAsync()
    {
        EnsureAdminPermission();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching all users...", async ctx =>
            {
                var users = await _userService.GetAllUsersAsync();
                DisplayUsers(users, "All Users");
            });
    }

    private async Task ViewUserDetailsAsync()
    {
        if (!EnsureAdminPermission())
            return;

        var users = await FetchAndSelectUserAsync("View User Details");
        if (users == null)
            return;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Fetching details for user {users.Email}...", async ctx =>
            {
                var user = await _userService.GetUserByIdAsync(users.GoogleId);
                if (user != null)
                {
                    DisplayUserDetails(user, "User Details");
                }
            });
    }

private async Task SearchUsersAsync()
{
    if (!EnsureAdminPermission())
        return;

    var searchCriteria = new UserSearchCriteria();

    var criteriaOptions = new List<string>
    {
        "Name (searches first name, last name, and email)",
        "First Name",
        "Last Name",
        "Email",
        "Role",
        "Active Status"
    };

    var selectedCriteria = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
            .Title("[yellow]Select search criteria to include[/]")
            .PageSize(10)
            .InstructionsText("[grey](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
            .AddChoices(criteriaOptions));

    var criteriaActions = new Dictionary<string, Action>
    {
        ["Name (searches first name, last name, and email)"] = () =>
            searchCriteria.Name = InputHelper.AskForString("[cyan]Enter name to search for:[/]"),

        ["First Name"] = () =>
            searchCriteria.FirstName = InputHelper.AskForString("[cyan]Enter first name:[/]"),

        ["Last Name"] = () =>
            searchCriteria.LastName = InputHelper.AskForString("[cyan]Enter last name:[/]"),

        ["Email"] = () =>
            searchCriteria.Email = InputHelper.AskForString("[cyan]Enter email:[/]"),

        ["Role"] = () =>
            searchCriteria.Role = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Select role to filter by[/]")
                    .PageSize(10)
                    .AddChoices(_availableRoles)),

        ["Active Status"] = () =>
            searchCriteria.IsActive = InputHelper.AskForConfirmation("[cyan]Show only active users?[/]")
    };

    foreach (var criterion in selectedCriteria)
    {
        if (criteriaActions.TryGetValue(criterion, out var action))
            action();
    }

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("green bold"))
        .StartAsync("[yellow]Searching users...[/]", async ctx =>
        {
            var users = await _userService.SearchUsersAsync(searchCriteria);

            var title = new StringBuilder("[bold blue]Search Results for Users[/]");
            if (!string.IsNullOrEmpty(searchCriteria.Name))
                title.Append($" with [yellow]name[/] containing '[green]{searchCriteria.Name}[/]'");
            if (!string.IsNullOrEmpty(searchCriteria.FirstName))
                title.Append($" with [yellow]first name[/] containing '[green]{searchCriteria.FirstName}[/]'");
            if (!string.IsNullOrEmpty(searchCriteria.LastName))
                title.Append($" with [yellow]last name[/] containing '[green]{searchCriteria.LastName}[/]'");
            if (!string.IsNullOrEmpty(searchCriteria.Email))
                title.Append($" with [yellow]email[/] containing '[green]{searchCriteria.Email}[/]'");
            if (!string.IsNullOrEmpty(searchCriteria.Role))
                title.Append($" with [yellow]role[/] '[green]{searchCriteria.Role}[/]'");
            if (searchCriteria.IsActive.HasValue)
                title.Append($" ([yellow]Status[/]: [green]{(searchCriteria.IsActive.Value ? "Active" : "Inactive")}[/])");

            DisplayUsers(users, title.ToString());
        });
}


    private async Task ActivateUserAsync()
    {
        if (!EnsureAdminPermission())
            return;

        var users = await FetchAndSelectInactiveUserAsync("Activate User");
        if (users == null)
            return;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Activating user {users.Email}...", async ctx =>
            {
                var result = await _userService.ActivateUserAsync(users.GoogleId);
                if (result)
                {
                    AnsiConsole.MarkupLine($"[green]User {users.Email} activated successfully![/]");

                    var updatedUser = await _userService.GetUserByIdAsync(users.GoogleId);
                    if (updatedUser != null)
                    {
                        DisplayUserDetails(updatedUser, "Updated User Details");
                    }
                }
            });
    }

    private async Task DeactivateUserAsync()
    {
        if (!EnsureAdminPermission())
            return;

        var users = await FetchAndSelectActiveUserAsync("Deactivate User");
        if (users == null)
            return;

        if (users.GoogleId == _context.CurrentUser.GoogleId)
        {
            AnsiConsole.MarkupLine("[red]You cannot deactivate your own account.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Deactivating user {users.Email}...", async ctx =>
            {
                var result = await _userService.DeactivateUserAsync(users.GoogleId);
                if (result)
                {
                    AnsiConsole.MarkupLine($"[green]User {users.Email} deactivated successfully![/]");

                    var updatedUser = await _userService.GetUserByIdAsync(users.GoogleId);
                    if (updatedUser != null)
                    {
                        DisplayUserDetails(updatedUser, "Updated User Details");
                    }
                }
            });
    }

    private async Task UpdateUserRoleAsync()
    {
        if (!EnsureAdminPermission())
            return;

        var users = await FetchAndSelectUserAsync("Update User Role");
        if (users == null)
            return;

        if (users.GoogleId == _context.CurrentUser.GoogleId)
        {
            AnsiConsole.MarkupLine("[red]You cannot change your own role.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"Current Role: [yellow]{users.Role}[/]");

        var newRole = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select new role")
                .AddChoices(_availableRoles));

        if (newRole == users.Role)
        {
            AnsiConsole.MarkupLine("[yellow]No change in role. Operation canceled.[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync($"Updating role for user {users.Email} to {newRole}...", async ctx =>
            {
                var result = await _userService.UpdateUserRoleAsync(users.GoogleId, newRole);
                if (result)
                {
                    AnsiConsole.MarkupLine($"[green]User role updated successfully to {newRole}![/]");

                    var updatedUser = await _userService.GetUserByIdAsync(users.GoogleId);
                    if (updatedUser != null)
                    {
                        DisplayUserDetails(updatedUser, "Updated User Details");
                    }
                }
            });
    }

    private async Task<UserInfo?> FetchAndSelectUserAsync(string title)
    {
        var users = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching users...", async _ => await _userService.GetAllUsersAsync());

        if (!users.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No users found.[/]");
            return null;
        }

        return SelectUser(users, title);
    }

    private async Task<UserInfo?> FetchAndSelectActiveUserAsync(string title)
    {
        var allUsers = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching active users...", async _ => await _userService.GetAllUsersAsync());

        var activeUsers = allUsers.Where(u => u.IsActive).ToList();

        if (!activeUsers.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No active users found.[/]");
            return null;
        }

        return SelectUser(activeUsers, title);
    }

    private async Task<UserInfo?> FetchAndSelectInactiveUserAsync(string title)
    {
        var allUsers = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Fetching inactive users...", async _ => await _userService.GetAllUsersAsync());

        var inactiveUsers = allUsers.Where(u => !u.IsActive).ToList();

        if (!inactiveUsers.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No inactive users found.[/]");
            return null;
        }

        return SelectUser(inactiveUsers, title);
    }

    private UserInfo? SelectUser(IEnumerable<UserInfo> users, string title)
    {
        return SelectionHelper.SelectFromList(
            users,
            u => $"{u.FirstName} {u.LastName} ({u.Email}) - {u.Role}",
            title);
    }

    private void DisplayUsers(IEnumerable<UserInfo> users, string title)
    {
        var userList = users.ToList();

        var columns = new[] { "Google ID", "Name", "Email", "Role", "Status" };

        var rows = userList.Select(u => new[]
        {
            u.GoogleId,
            $"{u.FirstName} {u.LastName}",
            u.Email,
            u.Role,
            DisplayHelper.FormatActiveStatus(u.IsActive)
        });

        DisplayHelper.DisplayTable(title, columns, rows);
    }

    private void DisplayUserDetails(UserInfo user, string title)
    {
        var details = new Dictionary<string, string>
        {
            ["Google ID"] = user.GoogleId,
            ["First Name"] = user.FirstName,
            ["Last Name"] = user.LastName,
            ["Email"] = user.Email,
            ["Role"] = user.Role,
            ["Status"] = DisplayHelper.FormatActiveStatus(user.IsActive)
        };

        DisplayHelper.DisplayDetails(title, details);
    }

    private bool EnsureAdminPermission()
    {
        if (_context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine("[red]You don't have permission to manage users. Admin role required.[/]");
            return false;
        }
        return true;
    }
}