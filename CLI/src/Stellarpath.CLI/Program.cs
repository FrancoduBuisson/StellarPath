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
                var categories = processor.GetCommandCategories();
                var commandDescriptions = processor.GetCommandDescriptions();
                var categoryDescriptions = processor.GetCategoryDescriptions();

                string selectedCategory = UiHelper.ShowMainMenu(context.CurrentUser, categories);

                if (categories.TryGetValue(selectedCategory, out var category))
                {
                    string selectedCommand = UiHelper.ShowCategoryMenu(selectedCategory, category, commandDescriptions);

                    if (!string.IsNullOrEmpty(selectedCommand))
                    {
                        exitRequested = await processor.ProcessCommandAsync(selectedCommand);
                    }
                }
            }
        }

        UiHelper.ShowGoodbyeMessage();
    }
}