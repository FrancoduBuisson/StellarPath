using Stellarpath.CLI.Core;
using Stellarpath.CLI.Services;
using Stellarpath.CLI.UI;

namespace StellarPath.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        var context = new CommandContext();
        var authService = new AuthService(context);
        var processor = new CommandProcessor(context, authService);

        UiHelper.ShowWelcomeMessage();

        bool exitRequested = false;
        while (!exitRequested)
        {
            if (!context.IsLoggedIn)
            {
                string choice = UiHelper.ShowLoginMenu();
                switch (choice)
                {
                    case "login":
                        await authService.LoginAsync();
                        break;
                    case "exit":
                        exitRequested = true;
                        break;
                }
            }
            else
            {
                string input = UiHelper.PromptUserInput(context.CurrentUser);
                exitRequested = await processor.ProcessCommandAsync(input);
            }
        }

        UiHelper.ShowGoodbyeMessage();
    }
}
