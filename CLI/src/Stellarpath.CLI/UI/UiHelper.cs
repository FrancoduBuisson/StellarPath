using System;
using System.Collections.Generic;
using Spectre.Console;
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
