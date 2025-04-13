using System.Text;
using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class DisplayHelper
{
    public static void DisplayTable(string title, string[] columns, IEnumerable<string[]> rows)
    {
        var rowsList = rows.ToList();
        if (!rowsList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No items found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.DarkCyan)
            .Title($"[bold blue]{title}[/]")
            .Caption($"[grey]Total: {rowsList.Count} items[/]")
            .Expand();

        foreach (var column in columns)
        {
            table.AddColumn(new TableColumn($"[bold]{column}[/]"));
        }

        foreach (var row in rowsList)
        {
            var styledRow = row.Select((cell, index) => StyleCell(cell, columns[index])).ToArray();
            table.AddRow(styledRow);
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayDetails(string title, Dictionary<string, string> details)
    {
        if (details == null || !details.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No details to display.[/]");
            return;
        }

        var content = new StringBuilder();

        foreach (var detail in details)
        {
            string styledValue = StyleCell(detail.Value, detail.Key);
            content.AppendLine($"[bold blue]{detail.Key}:[/] {styledValue}");
        }

        var panel = new Panel(new Markup(content.ToString()))
        {
            Header = new PanelHeader($"[bold blue]{title}[/]"),
            Border = BoxBorder.Rounded,
            Expand = true,
            Padding = new Padding(2, 1, 2, 1)
        };

        AnsiConsole.Write(panel);
    }

    private static string StyleCell(string value, string column)
    {
        if (column.Contains("Status", StringComparison.OrdinalIgnoreCase))
        {
            return value switch
            {
                var v when v.Contains("Active", StringComparison.OrdinalIgnoreCase) => "[green]Active[/]",
                var v when v.Contains("Inactive", StringComparison.OrdinalIgnoreCase) => "[yellow]Inactive[/]",
                var v when v.Contains("Scheduled", StringComparison.OrdinalIgnoreCase) => "[blue]Scheduled[/]",
                var v when v.Contains("In Progress", StringComparison.OrdinalIgnoreCase) => "[green]In Progress[/]",
                var v when v.Contains("Completed", StringComparison.OrdinalIgnoreCase) => "[cyan]Completed[/]",
                var v when v.Contains("Cancelled", StringComparison.OrdinalIgnoreCase) => "[red]Cancelled[/]",
                _ => value
            };
        }

        if (column.Equals("Price", StringComparison.OrdinalIgnoreCase) || value.StartsWith("$"))
        {
            return $"[cyan]{value}[/]";
        }

        if (column.Equals("ID", StringComparison.OrdinalIgnoreCase))
        {
            return $"[grey]{value}[/]";
        }

        return value;
    }

    public static string FormatActiveStatus(bool isActive)
        => isActive ? "[green]Active[/]" : "[yellow]Inactive[/]";

    public static string FormatDateTime(DateTime dateTime)
        => dateTime.ToString("yyyy-MM-dd HH:mm");

    public static string FormatPrice(decimal price)
        => $"[cyan]{price:C2}[/]";
}
