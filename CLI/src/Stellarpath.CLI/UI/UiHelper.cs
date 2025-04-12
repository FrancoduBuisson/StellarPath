using System;
using System.Collections.Generic;
using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.UI;

public static class UiHelper
{
    public static void ShowWelcomeMessage()
    {
        AnsiConsole.Write(new FigletText("StellarPath CLI").Color(Color.Green));
        AnsiConsole.MarkupLine("[grey]Welcome to the StellarPath Console Client[/]\n");
    }

    public static void ShowGoodbyeMessage()
    {
        AnsiConsole.MarkupLine("[yellow]Thank you for using StellarPath CLI. Goodbye![/]");
    }

    public static string ShowLoginMenu()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose an option")
                .AddChoices("login", "exit"));
    }

    public static string ShowMainMenu(UserInfo user, Dictionary<string, CommandCategory> categories)
    {
        var categoryDescriptions = CommandMenuStructure.GetCategoryDescriptions();

        var displayOptions = new List<string>();
        foreach (var category in categories.Keys)
        {
            string description = categoryDescriptions.TryGetValue(category, out var desc) ? desc : "";
            displayOptions.Add($"{category} - {description}");
        }

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[green]{user?.FirstName ?? "Guest"}[/]: Select a category")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(displayOptions));

        return selection.Split(" - ")[0].Trim();
    }

    public static string ShowCategoryMenu(string categoryName, CommandCategory category, Dictionary<string, string> commandDescriptions)
    {
        var menuItems = new List<string>();

        foreach (var command in category.Commands)
        {
            string description = commandDescriptions.TryGetValue(command, out var desc) ? desc : command;
            menuItems.Add($"{command} - {description}");
        }

        menuItems.Add(CommandMenuStructure.BACK);

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[green]{categoryName}[/]: Select a command")
                .PageSize(12)
                .HighlightStyle(new Style(Color.Blue))
                .AddChoices(menuItems));

        if (selection == CommandMenuStructure.BACK)
            return null;

        return selection.Split(" - ")[0].Trim();
    }

    public static string PromptUserInput(UserInfo user)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"{user?.FirstName ?? "Guest"}: ").PromptStyle("green"));
    }

    public static void ShowUserInfo(UserInfo user)
    {
        if (user == null)
        {
            AnsiConsole.MarkupLine("[red]No user information found.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Field").AddColumn("Value");
        table.AddRow("Name", $"{user.FirstName} {user.LastName}");
        table.AddRow("Email", user.Email);
        table.AddRow("Role", user.Role);
        table.AddRow("Google ID", user.GoogleId);
        table.AddRow("Active", user.IsActive ? "Yes" : "No");

        AnsiConsole.Write(table);
    }
}