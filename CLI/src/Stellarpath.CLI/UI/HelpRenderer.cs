using Spectre.Console;
using Stellarpath.CLI.Core;

namespace Stellarpath.CLI.UI;

public static class HelpRenderer
{
    public static void ShowHelp()
    {
        var categoryDescriptions = CommandMenuStructure.GetCategoryDescriptions();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .Title("[bold blue]StellarPath CLI - Main Menu Categories[/]")
            .AddColumn(new TableColumn("[u]Category[/]").Width(20))
            .AddColumn(new TableColumn("[u]Description[/]"));

        foreach (var category in categoryDescriptions)
        {
            table.AddRow(
                $"[green]{category.Key}[/]",
                category.Value
            );
        }

        table.Caption("Use arrow keys to navigate, Enter to select, and Escape to go back.");

        AnsiConsole.Write(table);
    }
}