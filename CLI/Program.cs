using CLI.Command;
using CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{

    static async Task Main()
    {
        string command;
        PrintStellarPath();

        IHost _host = Host.CreateDefaultBuilder().ConfigureServices(
            services =>
            {
                services.AddSingleton<IApplication, Application>();
            })
            .Build();

        var app = _host.Services.GetRequiredService<IApplication>();
        app.Run();
        HttpClient client = new HttpClient();
        var loginCommand = new LoginCommand();
        do
        {
            Console.Write("Enter a command (type 'help' for assistance): ");
            command = Console.ReadLine()?.Trim().ToLower();
           

            switch (command)
            {

                case "login":
                    loginCommand.LoginAsync().Wait();
                    break;
                case "logout":
                    loginCommand.Logout();
                    break;
                case "quit":
                    Console.WriteLine("Exiting program...");
                    break;

                case "help":
                    PrintStellarPathHelp();
                    break;
                case "clear":
                    Console.Clear();
                    PrintStellarPath();
                    break;

                default:
                    Console.WriteLine("Unsupported command. Type 'help' for accepted commands.");
                    break;
            }

        } while (command.ToLower() != "quit" && command.ToLower() != "exit");
        
        Console.WriteLine("Good bye...."); 
    }

    static void PrintStellarPath()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.BackgroundColor = ConsoleColor.Black;

        string bannerText = @"
        ╔══════════════════════════════════════════════════════════════════════════════════════════╗
        ║                                                                                          ║
        ║                                     STELLAR PATH CLI                                     ║
        ║                                                                                          ║
        ╚══════════════════════════════════════════════════════════════════════════════════════════╝
        ";                                                
        Console.WriteLine(bannerText);
        Console.ResetColor();
    }

    static void PrintStellarPathHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.BackgroundColor = ConsoleColor.Black;

        string helpBannerText = @"
        ╔══════════════════════════════════════════════════════════════════════════════════════════╗
        ║                                                                                          ║
        ║                                    STELLAR PATH CLI HELP                                 ║
        ║                                                                                          ║
        ╠══════════════════════════════════════════════════════════════════════════════════════════╣
        ║ COMMANDS:                                                                                ║
        ║    login       - Logs in user to application.                                            ║
        ║    logout      - Logs out user from application.                                         ║
        ║    help        - Displays commands you can execute                                       ║
        ║    quit        - Stops the application.                                                  ║
        ║    exit        - Stops the application.                                                  ║
        ║    clear       - Clears console.                                                         ║
        ║                                                                                          ║
        ╚══════════════════════════════════════════════════════════════════════════════════════════╝
        ";
        Console.WriteLine(helpBannerText);
        Console.ResetColor();

    }
}
