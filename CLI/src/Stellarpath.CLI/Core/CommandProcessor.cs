using Spectre.Console;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Core;

public class CommandProcessor
{
    private readonly CommandContext _context;
    private readonly AuthService _authService;

    public CommandProcessor(CommandContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<bool> ProcessCommandAsync(string input)
    {
        string command = input.Trim().ToLower();
        switch (command)
        {
            case "help":
                HelpRenderer.ShowHelp();
                break;
            case "whoami":
                UiHelper.ShowUserInfo(_context.CurrentUser);
                break;
            case "logout":
                _authService.Logout();
                break;
            case "exit":
            case "quit":
                return true;
            default:
                AnsiConsole.MarkupLine($"[yellow]Unknown command: {command}. Type 'help' for options.[/]");
                break;
        }

        return false;
    }
}
