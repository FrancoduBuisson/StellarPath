using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class HelpRenderer
{
    public static void ShowHelp()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title("[bold blue]Available Commands[/]")
            .AddColumn(new TableColumn("[u]Command[/]").Width(15))
            .AddColumn(new TableColumn("[u]Description[/]"));

        table.AddRow(
            "[green]help[/]",
            "Show this help message"
        );

        table.AddRow(
            "[green]whoami[/]",
            "Show logged-in user info"
        );

        table.AddRow(
            "[green]galaxies[/]",
            "Manage galaxies (view, search, create, update, activate/deactivate)"
        );

        table.AddRow(
            "[green]starsystems[/]",
            "Manage star systems (view, search, create, update, activate/deactivate)"
        );

        table.AddRow(
            "[green]destinations[/]",
            "Manage destinations (view, search, create, update, activate/deactivate)"
        );

        table.AddRow(
            "[green]shipmodels[/]",
            "Manage ship models (view, search, create, update)"
        );

        table.AddRow(
            "[green]spaceships[/]",
            "Manage spaceships (view, search, create, update, activate/deactivate, check availability)"
        );

        table.AddRow(
            "[green]logout[/]",
            "Logout of current session"
        );

        table.AddRow(
            "[green]exit[/] or [green]quit[/]",
            "Exit the CLI"
        );

        AnsiConsole.Write(table);
    }
}