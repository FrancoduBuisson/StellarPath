﻿using Spectre.Console;
using Stellarpath.CLI.Commands;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace Stellarpath.CLI.Core;

public class CommandProcessor
{
    private readonly CommandContext _context;
    private readonly AuthService _authService;
    private readonly GalaxyService _galaxyService;
    private readonly StarSystemService _starSystemService;
    private readonly DestinationService _destinationService;

    private readonly GalaxyCommandHandler _galaxyCommandHandler;
    private readonly StarSystemCommandHandler _starSystemCommandHandler;
    private readonly DestinationCommandHandler _destinationCommandHandler;

    public CommandProcessor(CommandContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;

        _galaxyService = new GalaxyService(context);
        _starSystemService = new StarSystemService(context);
        _destinationService = new DestinationService(context);

        _galaxyCommandHandler = new GalaxyCommandHandler(context, _galaxyService);
        _starSystemCommandHandler = new StarSystemCommandHandler(context, _starSystemService, _galaxyService);
        _destinationCommandHandler = new DestinationCommandHandler(context, _destinationService, _starSystemService);
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
            case "galaxies":
                await _galaxyCommandHandler.HandleAsync();
                break;
            case "starsystems":
                await _starSystemCommandHandler.HandleAsync();
                break;
            case "destinations":
                await _destinationCommandHandler.HandleAsync();
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