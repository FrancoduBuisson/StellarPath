using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class SelectionHelper
{
    public static T SelectFromList<T>(
        IEnumerable<T> items,
        Func<T, string> displaySelector,
        string title,
        int pageSize = 10) where T : class
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No items available for selection.[/]");
            return null;
        }

        var options = itemList.Select(item => displaySelector(item)).ToList();
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(pageSize)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        return itemList[options.IndexOf(selectedOption)];
    }

    public static T SelectFromListById<T, TId>(
        IEnumerable<T> items,
        Func<T, TId> idSelector,
        Func<T, string> displaySelector,
        string title,
        int pageSize = 10) where T : class
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No items available for selection.[/]");
            return null;
        }

        var options = itemList.Select(item => $"{idSelector(item)}: {displaySelector(item)}").ToList();
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(pageSize)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        string idPart = selectedOption.Split(':')[0].Trim();
        TId selectedId;

        if (typeof(TId) == typeof(int))
        {
            selectedId = (TId)(object)int.Parse(idPart);
        }
        else if (typeof(TId) == typeof(string))
        {
            selectedId = (TId)(object)idPart;
        }
        else if (typeof(TId) == typeof(Guid))
        {
            selectedId = (TId)(object)Guid.Parse(idPart);
        }
        else
        {
            throw new ArgumentException($"Unsupported ID type: {typeof(TId).Name}");
        }

        return itemList.FirstOrDefault(item => EqualityComparer<TId>.Default.Equals(idSelector(item), selectedId));
    }

}