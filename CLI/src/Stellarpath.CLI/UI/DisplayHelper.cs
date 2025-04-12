using System.Text;
using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class DisplayHelper
{
    public static void DisplayTable(
        string title,
        string[] columns,
        IEnumerable<string[]> rows)
    {
        var rowsList = rows.ToList();

        if (!rowsList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No items found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .Title($"[bold green]{title}[/]")
            .Caption($"[grey]Total: {rowsList.Count}[/]");

        // Add columns
        foreach (var column in columns)
        {
            table.AddColumn(new TableColumn($"[u]{column}[/]"));
        }

        // Add rows
        foreach (var row in rowsList)
        {
            table.AddRow(row);
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayDetails(
        string title,
        Dictionary<string, string> details)
    {
        if (details == null || !details.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No details to display.[/]");
            return;
        }

        var content = new StringBuilder();

        foreach (var detail in details)
        {
            content.AppendLine($"[bold]{detail.Key}:[/] {detail.Value}");
        }

        var panel = new Panel(new Markup(content.ToString()))
        {
            Header = new PanelHeader(title),
            Border = BoxBorder.Rounded,
            Expand = true,
            Padding = new Padding(1, 1, 1, 1)
        };

        AnsiConsole.Write(panel);
    }

    public static string FormatActiveStatus(bool isActive)
    {
        return isActive ? "[green]Active[/]" : "[yellow]Inactive[/]";
    }

    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm");
    }

    public static string FormatPrice(decimal price)
    {
        return $"{price:C2}";
    }
}