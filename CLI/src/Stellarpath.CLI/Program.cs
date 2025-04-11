using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Spectre.Console;
using System.Diagnostics;
using Stellarpath.CLI.Models;
using StellarPath.CLI.Utility;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using static Stellarpath.CLI.Commands.SpaceshipDetailCommand;
using Stellarpath.CLI.Commands;
using Stellarpath.CLI.Utility;

namespace StellarPath.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Show welcome message
            ShowWelcomeMessage();

            // Set up DI services
            var services = new ServiceCollection();

            // Configure services
            ConfigureServices(services);

            // Registering commands with the DI container
            var registrar = new TypeRegistrar(services);

            // Create a new command app
            var app = new CommandApp(registrar);

            // Configure the app
            ConfigureApp(app);

            // Run the app
            return await app.RunAsync(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // HTTP client that will be used for API calls
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7029")  // Set your API base URL here
            };

            // API client for making calls to our backend
            var apiClient = new ApiClient(httpClient);

            // Application state to track login status and user info
            var appState = new AppState();

            // Try to load existing session
            var session = SessionManager.LoadSession();
            if (session != null && !string.IsNullOrEmpty(session.JwtToken) && session.CurrentUser != null)
            {
                appState.JwtToken = session.JwtToken;
                appState.CurrentUser = session.CurrentUser;
                appState.IsLoggedIn = true;
                apiClient.SetAuthToken(appState.JwtToken);

                AnsiConsole.MarkupLine($"[green]Welcome back, {appState.CurrentUser.FirstName} {appState.CurrentUser.LastName}![/]");
            }

            // Register these as singletons in our DI container
            services.AddSingleton(httpClient);
            services.AddSingleton(apiClient);
            services.AddSingleton(appState);

            // Register command classes
            services.AddTransient<LoginCommand>();
            services.AddTransient<LogoutCommand>();
            services.AddTransient<WhoAmICommand>();
            services.AddTransient<ListSpaceshipsCommand>();
            services.AddTransient<SpaceshipDetailCommand>();
            services.AddTransient<ListGalaxiesCommand>();
            services.AddTransient<ListStarSystemsCommand>();
            services.AddTransient<ListDestinationsCommand>();
        }

        private static void ConfigureApp(CommandApp app)
        {
            // Configure the app and add commands
            app.Configure(config =>
            {
                // Set application name and version
                config.SetApplicationName("stellarpath");
                config.SetApplicationVersion("1.0.0");

                // Add the login command
                config.AddCommand<LoginCommand>("login")
                    .WithDescription("Log in to StellarPath with Google authentication");

                // Add the logout command
                config.AddCommand<LogoutCommand>("logout")
                    .WithDescription("Log out from StellarPath");

                // Add the whoami command
                config.AddCommand<WhoAmICommand>("whoami")
                    .WithDescription("Display information about the currently logged in user");

                // Add spaceship commands
                config.AddBranch("spaceships", spaceship =>
                {
                    spaceship.SetDescription("Manage and view spaceships");

                    spaceship.AddCommand<ListSpaceshipsCommand>("list")
                        .WithDescription("List all available spaceships");

                    spaceship.AddCommand<SpaceshipDetailCommand>("detail")
                        .WithDescription("View details of a specific spaceship");
                });

                // Add cosmic object commands
                config.AddBranch("galaxies", galaxies =>
                {
                    galaxies.SetDescription("View galaxies");

                    galaxies.AddCommand<ListGalaxiesCommand>("list")
                        .WithDescription("List all available galaxies");
                });

                config.AddBranch("systems", systems =>
                {
                    systems.SetDescription("View star systems");

                    systems.AddCommand<ListStarSystemsCommand>("list")
                        .WithDescription("List all star systems");
                });

                config.AddBranch("destinations", destinations =>
                {
                    destinations.SetDescription("View destinations");

                    destinations.AddCommand<ListDestinationsCommand>("list")
                        .WithDescription("List all destinations");
                });

                
            });
        }

        private static void ShowWelcomeMessage()
        {
            AnsiConsole.Write(
                new FigletText("StellarPath CLI")
                    .LeftJustified()
                    .Color(Color.Green));

            AnsiConsole.MarkupLine("[grey]Welcome to the StellarPath Console Client[/]");
            AnsiConsole.WriteLine();
        }

        private static async Task<int> RunInteractiveMode(
            CommandContext context,
            IAnsiConsole console,
            ITypeResolver typeResolver)
        {
            var appState = typeResolver.Resolve(typeof(AppState)) as AppState;
            var apiClient = typeResolver.Resolve(typeof(ApiClient)) as ApiClient;
            var httpClient = typeResolver.Resolve(typeof(HttpClient)) as HttpClient;

            if (appState == null || apiClient == null || httpClient == null)
            {
                console.MarkupLine("[red]Failed to initialize CLI services.[/]");
                return 1;
            }

            bool exitRequested = false;

            console.MarkupLine("[blue]Type 'help' to see available commands, or 'exit' to quit.[/]");
            console.WriteLine();

            while (!exitRequested)
            {
                try
                {
                    if (!appState.IsLoggedIn)
                    {
                        // Show login menu when not logged in
                        var loginChoice = console.Prompt(
                            new SelectionPrompt<string>()
                                .Title("What would you like to do?")
                                .PageSize(10)
                                .AddChoices(new[] {
                                    "login", "exit"
                                }));

                        switch (loginChoice)
                        {
                            case "login":
                                var loginHandler = new LoginHandler(httpClient);
                                var authResult = await loginHandler.LoginAsync();

                                if (authResult.Success)
                                {
                                    appState.JwtToken = authResult.Token;
                                    appState.CurrentUser = authResult.User;
                                    appState.IsLoggedIn = true;
                                    apiClient.SetAuthToken(appState.JwtToken);

                                    // Display a welcome message with the user's name
                                    console.MarkupLine($"[green]Welcome, {appState.CurrentUser.FirstName} {appState.CurrentUser.LastName}![/]");
                                    console.MarkupLine($"[grey]Role: {appState.CurrentUser.Role}[/]");
                                    console.WriteLine();
                                    ShowHelpMessage(console);
                                }
                                break;

                            case "exit":
                                exitRequested = true;
                                break;
                        }
                    }
                    else
                    {
                        // Main interactive prompt when logged in
                        string input = console.Prompt(
                            new TextPrompt<string>($"{appState.CurrentUser.FirstName}> ")
                                .PromptStyle("green"));

                        await ProcessCommandAsync(input, console, context, typeResolver, appState, apiClient);

                        // Check if exit requested
                        if (input.ToLower() == "exit" || input.ToLower() == "quit")
                        {
                            exitRequested = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    console.MarkupLine($"[red]Error: {ex.Message}[/]");
                }
            }

            console.MarkupLine("[yellow]Thank you for using StellarPath CLI. Goodbye![/]");
            return 0;
        }

        private static async Task ProcessCommandAsync(
            string input,
            IAnsiConsole console,
            CommandContext context,
            ITypeResolver typeResolver,
            AppState appState,
            ApiClient apiClient)
        {
            string command = input.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(command))
                return;

            switch (command)
            {
                case "help":
                    ShowHelpMessage(console);
                    break;

                case "logout":
                    var logoutCommand = typeResolver.Resolve(typeof(LogoutCommand)) as LogoutCommand;
                    if (logoutCommand != null)
                    {
                        logoutCommand.Execute(context, new LogoutCommandSettings());
                    }
                    else
                    {
                        // Fallback if DI fails
                        appState.JwtToken = null;
                        appState.CurrentUser = null;
                        appState.IsLoggedIn = false;
                        apiClient.ClearAuthToken();
                        SessionManager.ClearSession();
                        console.MarkupLine("[green]Logged out successfully.[/]");
                    }
                    break;

                case "whoami":
                    var whoamiCommand = typeResolver.Resolve(typeof(WhoAmICommand)) as WhoAmICommand;
                    if (whoamiCommand != null)
                    {
                        whoamiCommand.Execute(context, new WhoAmICommandSettings());
                    }
                    else
                    {
                        // Fallback if DI fails
                        ShowUserInfo(console, appState);
                    }
                    break;

                case "exit":
                case "quit":
                    // Handled in main loop
                    break;

                default:
                    // Try to parse as a command with arguments
                    var parts = command.Split(' ', 2);
                    string mainCommand = parts[0];
                    string[] args = parts.Length > 1
                        ? parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        : Array.Empty<string>();

                    switch (mainCommand)
                    {
                        case "spaceships":
                            await HandleSpaceshipsCommand(args, console, context, typeResolver);
                            break;

                        case "galaxies":
                            await HandleGalaxiesCommand(args, console, context, typeResolver);
                            break;

                        case "systems":
                            await HandleSystemsCommand(args, console, context, typeResolver);
                            break;

                        case "destinations":
                            await HandleDestinationsCommand(args, console, context, typeResolver);
                            break;

                        default:
                            console.MarkupLine($"[yellow]Unknown command: {command}. Type 'help' for available commands.[/]");
                            break;
                    }
                    break;
            }
        }

        private static async Task HandleSpaceshipsCommand(
            string[] args,
            IAnsiConsole console,
            CommandContext context,
            ITypeResolver typeResolver)
        {
            if (args.Length == 0)
            {
                console.MarkupLine("[yellow]Please specify a spaceship subcommand: list, detail[/]");
                return;
            }

            switch (args[0])
            {
                case "list":
                    var listCommand = typeResolver.Resolve(typeof(ListSpaceshipsCommand)) as ListSpaceshipsCommand;
                    if (listCommand != null)
                    {
                        var settings = new ListSpaceshipsCommandSettings();
                        // Parse additional args if needed
                        if (args.Contains("-a") || args.Contains("--active"))
                        {
                            settings.ActiveOnly = true;
                        }

                        var modelIdIndex = Array.IndexOf(args, "-m");
                        if (modelIdIndex == -1)
                        {
                            modelIdIndex = Array.IndexOf(args, "--model");
                        }

                        if (modelIdIndex >= 0 && modelIdIndex < args.Length - 1 && int.TryParse(args[modelIdIndex + 1], out int modelId))
                        {
                            settings.ModelId = modelId;
                        }

                        await listCommand.ExecuteAsync(context, settings);
                    }
                    break;

                case "detail":
                    if (args.Length < 2)
                    {
                        console.MarkupLine("[yellow]Please provide a spaceship ID.[/]");
                        return;
                    }

                    if (int.TryParse(args[1], out int spaceshipId))
                    {
                        var detailCommand = typeResolver.Resolve(typeof(SpaceshipDetailCommand)) as SpaceshipDetailCommand;
                        if (detailCommand != null)
                        {
                            var settings = new SpaceshipDetailCommandSettings
                            {
                                SpaceshipId = spaceshipId
                            };

                            await detailCommand.ExecuteAsync(context, settings);
                        }
                    }
                    else
                    {
                        console.MarkupLine("[yellow]Invalid spaceship ID. Please provide a valid number.[/]");
                    }
                    break;

                default:
                    console.MarkupLine($"[yellow]Unknown spaceship subcommand: {args[0]}. Use list or detail.[/]");
                    break;
            }
        }

        private static async Task HandleGalaxiesCommand(
            string[] args,
            IAnsiConsole console,
            CommandContext context,
            ITypeResolver typeResolver)
        {
            if (args.Length == 0)
            {
                console.MarkupLine("[yellow]Please specify a galaxies subcommand: list[/]");
                return;
            }

            switch (args[0])
            {
                case "list":
                    var listCommand = typeResolver.Resolve(typeof(ListGalaxiesCommand)) as ListGalaxiesCommand;
                    if (listCommand != null)
                    {
                        var settings = new ListGalaxiesCommandSettings();
                        // Parse additional args if needed
                        if (args.Contains("-a") || args.Contains("--active"))
                        {
                            settings.ActiveOnly = true;
                        }

                        await listCommand.ExecuteAsync(context, settings);
                    }
                    break;

                default:
                    console.MarkupLine($"[yellow]Unknown galaxies subcommand: {args[0]}. Use list.[/]");
                    break;
            }
        }

        private static async Task HandleSystemsCommand(
            string[] args,
            IAnsiConsole console,
            CommandContext context,
            ITypeResolver typeResolver)
        {
            if (args.Length == 0)
            {
                console.MarkupLine("[yellow]Please specify a systems subcommand: list[/]");
                return;
            }

            switch (args[0])
            {
                case "list":
                    var listCommand = typeResolver.Resolve(typeof(ListStarSystemsCommand)) as ListStarSystemsCommand;
                    if (listCommand != null)
                    {
                        var settings = new ListStarSystemsCommandSettings();

                        // Parse additional args if needed
                        if (args.Contains("-a") || args.Contains("--active"))
                        {
                            settings.ActiveOnly = true;
                        }

                        var galaxyIdIndex = Array.IndexOf(args, "-g");
                        if (galaxyIdIndex == -1)
                        {
                            galaxyIdIndex = Array.IndexOf(args, "--galaxy");
                        }

                        if (galaxyIdIndex >= 0 && galaxyIdIndex < args.Length - 1 && int.TryParse(args[galaxyIdIndex + 1], out int galaxyId))
                        {
                            settings.GalaxyId = galaxyId;
                        }

                        await listCommand.ExecuteAsync(context, settings);
                    }
                    break;

                default:
                    console.MarkupLine($"[yellow]Unknown systems subcommand: {args[0]}. Use list.[/]");
                    break;
            }
        }

        private static async Task HandleDestinationsCommand(
            string[] args,
            IAnsiConsole console,
            CommandContext context,
            ITypeResolver typeResolver)
        {
            if (args.Length == 0)
            {
                console.MarkupLine("[yellow]Please specify a destinations subcommand: list[/]");
                return;
            }

            switch (args[0])
            {
                case "list":
                    var listCommand = typeResolver.Resolve(typeof(ListDestinationsCommand)) as ListDestinationsCommand;
                    if (listCommand != null)
                    {
                        var settings = new ListDestinationsCommandSettings();

                        // Parse additional args if needed
                        if (args.Contains("-a") || args.Contains("--active"))
                        {
                            settings.ActiveOnly = true;
                        }

                        var systemIdIndex = Array.IndexOf(args, "-s");
                        if (systemIdIndex == -1)
                        {
                            systemIdIndex = Array.IndexOf(args, "--system");
                        }

                        if (systemIdIndex >= 0 && systemIdIndex < args.Length - 1 && int.TryParse(args[systemIdIndex + 1], out int systemId))
                        {
                            settings.SystemId = systemId;
                        }

                        await listCommand.ExecuteAsync(context, settings);
                    }
                    break;

                default:
                    console.MarkupLine($"[yellow]Unknown destinations subcommand: {args[0]}. Use list.[/]");
                    break;
            }
        }

        private static void ShowHelpMessage(IAnsiConsole console)
        {
            console.MarkupLine("[blue]Available commands:[/]");
            console.MarkupLine("  [green]help[/] - Show this help message");
            console.MarkupLine("  [green]whoami[/] - Show your user information");
            console.MarkupLine("  [green]logout[/] - Log out of the current session");
            console.MarkupLine("  [green]exit[/] or [green]quit[/] - Exit the application");

            console.MarkupLine("\n[blue]Spaceship commands:[/]");
            console.MarkupLine("  [green]spaceships list[/] - List all spaceships");
            console.MarkupLine("  [green]spaceships list --active[/] - List active spaceships only");
            console.MarkupLine("  [green]spaceships list --model <id>[/] - List spaceships by model ID");
            console.MarkupLine("  [green]spaceships detail <id>[/] - Show details of a specific spaceship");

            console.MarkupLine("\n[blue]Cosmic object commands:[/]");
            console.MarkupLine("  [green]galaxies list[/] - List all galaxies");
            console.MarkupLine("  [green]galaxies list --active[/] - List active galaxies only");
            console.MarkupLine("  [green]systems list[/] - List all star systems");
            console.MarkupLine("  [green]systems list --active[/] - List active star systems only");
            console.MarkupLine("  [green]systems list --galaxy <id>[/] - List star systems in a specific galaxy");
            console.MarkupLine("  [green]destinations list[/] - List all destinations");
            console.MarkupLine("  [green]destinations list --active[/] - List active destinations only");
            console.MarkupLine("  [green]destinations list --system <id>[/] - List destinations in a specific star system");

            console.WriteLine();
        }

        private static void ShowUserInfo(IAnsiConsole console, AppState appState)
        {
            if (appState.CurrentUser != null)
            {
                var table = new Table();
                table.AddColumn("Field");
                table.AddColumn("Value");

                table.AddRow("Name", $"{appState.CurrentUser.FirstName} {appState.CurrentUser.LastName}");
                table.AddRow("Email", appState.CurrentUser.Email);
                table.AddRow("Google ID", appState.CurrentUser.GoogleId);
                table.AddRow("Role", appState.CurrentUser.Role);
                table.AddRow("Active", appState.CurrentUser.IsActive ? "Yes" : "No");

                console.Write(table);
            }
            else
            {
                console.MarkupLine("[red]Not logged in.[/]");
            }
        }
    }

    // TypeRegistrar and TypeResolver for Spectre.Console DI support
    public sealed class TypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _builder;

        public TypeRegistrar(IServiceCollection builder)
        {
            _builder = builder;
        }

        public ITypeResolver Build()
        {
            return new TypeResolver(_builder.BuildServiceProvider());
        }

        public void Register(Type service, Type implementation)
        {
            _builder.AddSingleton(service, implementation);
        }

        public void RegisterInstance(Type service, object implementation)
        {
            _builder.AddSingleton(service, implementation);
        }

        public void RegisterLazy(Type service, Func<object> factory)
        {
            _builder.AddSingleton(service, _ => factory());
        }
    }

    public sealed class TypeResolver : ITypeResolver
    {
        private readonly IServiceProvider _provider;

        public TypeResolver(IServiceProvider provider)
        {
            _provider = provider;
        }

        public object Resolve(Type type)
        {
            return _provider.GetService(type);
        }
    }
}