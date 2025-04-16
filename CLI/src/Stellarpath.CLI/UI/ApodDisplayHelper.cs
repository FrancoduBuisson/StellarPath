using Spectre.Console;
using Stellarpath.CLI.Core;
using Stellarpath.CLI.Models;
using StellarPath.ConsoleClient.Services;
using System.Diagnostics;

namespace Stellarpath.CLI.UI
{
  public static class ApodDisplayHelper
  {
    public static void ShowApod(NasaApodResponse apod)
    {
      if (apod == null)
      {
        AnsiConsole.MarkupLine("[yellow]NASA API key not found or failed to fetch APOD.[/]");
        return;
      }

      AnsiConsole.WriteLine();

      var explanationShort = apod.Explanation.Length > 300
          ? apod.Explanation.Substring(0, 300) + "..."
          : apod.Explanation;

      AnsiConsole.Write(
          new Panel($"[bold]{apod.Title}[/]\n\n[blue underline]{apod.Url}[/]\n\n{explanationShort}")
              .Header("[yellow]NASA Picture of the Day[/]")
              .Border(BoxBorder.Rounded)
              .Padding(new Padding(1, 1, 1, 1))
              .Collapse()
      );

      AnsiConsole.WriteLine();

      if (apod.MediaType.IsImage)
      {
        var viewInBrowser = AnsiConsole.Confirm("Do you want to view the HD image in your browser?");
        if (viewInBrowser)
        {
          AnsiConsole.MarkupLine("[green]Opening HD image...[/]");
          Process.Start(new ProcessStartInfo
          {
            FileName = apod.HdUrl,
            UseShellExecute = true
          });
        }
      }
      else if (apod.MediaType.IsVideo)
      {
        AnsiConsole.MarkupLine("[yellow]Today's APOD is a video.[/]");
        var viewVideo = AnsiConsole.Confirm("Do you want to watch it in your browser?");
        if (viewVideo)
        {
          AnsiConsole.MarkupLine("[green]Opening video in your browser...[/]");
          Process.Start(new ProcessStartInfo
          {
            FileName = apod.Url,
            UseShellExecute = true
          });
        }
      }

      AnsiConsole.WriteLine();
    }
  }
}
