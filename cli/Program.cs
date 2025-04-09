using CLI.Command;
using CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data;
using System.Threading;

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
        do
        {
            Console.WriteLine("Enter a command (type 'quit' to exit):");
            command = Console.ReadLine()?.Trim().ToLower();

            switch (command)
            {

                case "login":
                    var loginCommand = new LoginCommand();
                    loginCommand.LoginAsync().Wait();
                    break;

                case "quit":
                    Console.WriteLine("Exiting program...");
                    break;

                default:
                    Console.WriteLine("Unknown command. Type 'quit' to exit or 'login' to log in.");
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
}
