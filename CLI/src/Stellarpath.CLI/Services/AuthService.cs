using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.UI;
using StellarPath.ConsoleClient.Services.Auth;

namespace Stellarpath.CLI.Services;

public class AuthService
{
    private readonly CommandContext _context;
    private readonly LoginHandler _loginHandler;

    public AuthService(CommandContext context)
    {
        _context = context;
        _loginHandler = new LoginHandler(context.HttpClient);
    }

    public async Task LoginAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync("Starting authentication process...", async ctx =>
            {
                var (idToken, accessToken) = await _loginHandler.GetGoogleAuth();
                if (string.IsNullOrEmpty(idToken))
                {
                    AnsiConsole.MarkupLine("[red]Failed to authenticate with Google.[/]");
                    return;
                }

                var result = await _loginHandler.AuthenticateWithBackend((idToken, accessToken));
                if (!result.Success)
                {
                    AnsiConsole.MarkupLine("[red]Failed to authenticate with backend.[/]");
                    return;
                }

                _context.SetAuth(result.Token, result.User);
                AnsiConsole.MarkupLine($"[green]Welcome, {result.User.FirstName}![/]");
                HelpRenderer.ShowHelp();
            });
    }

    public void Logout()
    {
        _context.ClearAuth();
        AnsiConsole.MarkupLine("[yellow]Logged out successfully.[/]");
    }
}

