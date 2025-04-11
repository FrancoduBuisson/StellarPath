using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class HelpRenderer
{
    public static void ShowHelp()
    {
        AnsiConsole.MarkupLine("[blue]Available commands:[/]");
        AnsiConsole.MarkupLine("  [green]help[/] - Show this help message");
        AnsiConsole.MarkupLine("  [green]whoami[/] - Show logged-in user info");
        AnsiConsole.MarkupLine("  [green]logout[/] - Logout of current session");
        AnsiConsole.MarkupLine("  [green]exit[/] or [green]quit[/] - Exit the CLI\n");
    }
}
