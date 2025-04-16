using Spectre.Console;
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
    private readonly ShipModelService _shipModelService;
    private readonly SpaceshipService _spaceshipService;
    private readonly CruiseService _cruiseService;
    private readonly UserService _userService;
    private readonly BookingService _bookingService;
    private readonly PlanetService _planetService;

    private readonly BookingCommandHandler _bookingCommandHandler;
    private readonly GalaxyCommandHandler _galaxyCommandHandler;
    private readonly StarSystemCommandHandler _starSystemCommandHandler;
    private readonly DestinationCommandHandler _destinationCommandHandler;
    private readonly ShipModelCommandHandler _shipModelCommandHandler;
    private readonly SpaceshipCommandHandler _spaceshipCommandHandler;
    private readonly CruiseCommandHandler _cruiseCommandHandler;
    private readonly UserCommandHandler _userCommandHandler;

    public CommandProcessor(CommandContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;

        _galaxyService = new GalaxyService(context);
        _starSystemService = new StarSystemService(context);
        _destinationService = new DestinationService(context);
        _shipModelService = new ShipModelService(context);
        _spaceshipService = new SpaceshipService(context);
        _cruiseService = new CruiseService(context);
        _userService = new UserService(context);
        _bookingService = new BookingService(context);
        _planetService = new PlanetService(context);


        _bookingCommandHandler = new BookingCommandHandler(context, _bookingService, _cruiseService, _userService);
        _galaxyCommandHandler = new GalaxyCommandHandler(context, _galaxyService);
        _starSystemCommandHandler = new StarSystemCommandHandler(context, _starSystemService, _galaxyService);
        _destinationCommandHandler = new DestinationCommandHandler(context,_planetService , _destinationService, _starSystemService);
        _shipModelCommandHandler = new ShipModelCommandHandler(context, _shipModelService);
        _spaceshipCommandHandler = new SpaceshipCommandHandler(context, _spaceshipService, _shipModelService);
        _cruiseCommandHandler = new CruiseCommandHandler(context, _cruiseService, _spaceshipService, _destinationService);
        _userCommandHandler = new UserCommandHandler(context, _userService);
    }

    public Dictionary<string, CommandCategory> GetCommandCategories()
    {
        bool isAdmin = _context.CurrentUser?.Role == "Admin";
        return CommandMenuStructure.GetCommandCategories(isAdmin);
    }

    public Dictionary<string, string> GetCommandDescriptions()
    {
        bool isAdmin = _context.CurrentUser?.Role == "Admin";
        return CommandMenuStructure.GetCommandDescriptions(isAdmin);
    }

    public Dictionary<string, string> GetCategoryDescriptions()
    {
        return CommandMenuStructure.GetCategoryDescriptions();
    }

    public async Task<bool> ProcessCommandAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        string command = input.Trim().ToLower();
        switch (command)
        {
            case CommandMenuStructure.CMD_HELP:
                HelpRenderer.ShowHelp();
                break;
            case CommandMenuStructure.CMD_WHOAMI:
                UiHelper.ShowUserInfo(_context.CurrentUser);
                break;
            case CommandMenuStructure.CMD_LOGOUT:
                _authService.Logout();
                break;
            case CommandMenuStructure.CMD_GALAXIES:
                await _galaxyCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_STARSYSTEMS:
                await _starSystemCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_DESTINATIONS:
                await _destinationCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_SHIPMODELS:
                await _shipModelCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_BOOKINGS:
                await _bookingCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_SPACESHIPS:
                await _spaceshipCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_CRUISES:
                await _cruiseCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_USERS:
                await _userCommandHandler.HandleAsync();
                break;
            case CommandMenuStructure.CMD_CLEAR:
                Console.Clear();
                break;
            case CommandMenuStructure.CMD_EXIT:
                return true;
            default:
                AnsiConsole.MarkupLine($"[yellow]Unknown command: {command}.[/]");
                break;
        }
        return false;
    }
}