using System.Collections.Generic;

namespace Stellarpath.CLI.Core;

public class CommandCategory
{
    public string Name { get; set; }
    public List<string> Commands { get; set; } = new List<string>();
}

public static class CommandMenuStructure
{
    // These are like our main menus
    public const string CELESTIAL_BODIES = "Celestial Bodies";
    public const string FLEET_MANAGEMENT = "Fleet Management";
    public const string TRAVEL = "Travel";
    public const string ACCOUNT_SYSTEM = "Account & System";
    public const string BACK = "<- Back to Main Menu";
    public const string CLEAR = "Clear";

    public const string CMD_GALAXIES = "galaxies";
    public const string CMD_STARSYSTEMS = "starsystems";
    public const string CMD_DESTINATIONS = "destinations";
    public const string CMD_SHIPMODELS = "shipmodels";
    public const string CMD_SPACESHIPS = "spaceships";
    public const string CMD_CRUISES = "cruises";
    public const string CMD_HELP = "help";
    public const string CMD_WHOAMI = "whoami";
    public const string CMD_USERS = "users";
    public const string CMD_LOGOUT = "logout";
    public const string CMD_EXIT = "exit";
    public const string CMD_CLEAR = "clear";
    public const string CMD_BOOKINGS = "bookings";

    public static Dictionary<string, CommandCategory> GetCommandCategories(bool isAdmin)
    {
        var categories = new Dictionary<string, CommandCategory>
        {
            [CELESTIAL_BODIES] = new CommandCategory
            {
                Name = CELESTIAL_BODIES,
                Commands = new List<string>
                {
                    CMD_GALAXIES,
                    CMD_STARSYSTEMS,
                    CMD_DESTINATIONS
                }
            },
            [FLEET_MANAGEMENT] = new CommandCategory
            {
                Name = FLEET_MANAGEMENT,
                Commands = new List<string>
                {
                    CMD_SHIPMODELS,
                    CMD_SPACESHIPS
                }
            },
            [TRAVEL] = new CommandCategory
            {
                Name = TRAVEL,
                Commands = new List<string>
                {
                    CMD_CRUISES,
                    CMD_BOOKINGS
                }
            },
            [ACCOUNT_SYSTEM] = new CommandCategory
            {
                Name = ACCOUNT_SYSTEM,
                Commands = new List<string>
                {
                    CMD_HELP,
                    CMD_WHOAMI,
                    CMD_LOGOUT,
                    CMD_EXIT
                }
            },
            [CLEAR] = new CommandCategory 
            { 
                Name = CLEAR,
                Commands = new List<string> 
                { 
                    CMD_CLEAR
                }
            }
        };

        if (isAdmin)
        {
            categories[ACCOUNT_SYSTEM].Commands.Insert(2, CMD_USERS);
        }

        return categories;
    }

    public static Dictionary<string, string> GetCommandDescriptions(bool isAdmin)
    {
        var actionWord = isAdmin ? "Manage" : "View";

        return new Dictionary<string, string>
        {
            [CMD_GALAXIES] = $"{actionWord} galaxies",
            [CMD_STARSYSTEMS] = $"{actionWord} star systems",
            [CMD_DESTINATIONS] = $"{actionWord} destinations",
            [CMD_SHIPMODELS] = $"{actionWord} ship models",
            [CMD_SPACESHIPS] = $"{actionWord} spaceships",
            [CMD_CRUISES] = $"{actionWord} cruises",
            [CMD_BOOKINGS] = $"{actionWord} bookings",
            [CMD_HELP] = "Show help",
            [CMD_WHOAMI] = "Show current user info",
            [CMD_USERS] = "Manage users",
            [CMD_LOGOUT] = "Logout from system",
            [CMD_EXIT] = "Exit application"

        };
    }

    public static Dictionary<string, string> GetCategoryDescriptions()
    {
        return new Dictionary<string, string>
        {
            [CELESTIAL_BODIES] = "Galaxies, Star Systems, and Destinations",
            [FLEET_MANAGEMENT] = "Ship Models and Spaceships",
            [TRAVEL] = "Cruises and Travel Management",
            [ACCOUNT_SYSTEM] = "Help, User Info, and System Functions",
            [CLEAR] = "Clear the console ouput"
        };
    }
}