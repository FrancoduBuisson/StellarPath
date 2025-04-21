using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Stellarpath.CLI.UI;

public static class EscapableInputHelper
{
    public class InputCancelledException : Exception
    {
        public InputCancelledException() : base("Input was cancelled by the user.") { }
    }

    public static string AskForString(
        string prompt,
        string defaultValue = null,
        Func<string, ValidationResult> validator = null,
        int maxSize = 100)
    {
        AnsiConsole.MarkupLine("[grey](Press [blue]Escape[/] to cancel and return)[/]");

        Console.WriteLine(prompt);

        if (!string.IsNullOrEmpty(defaultValue))
        {
            Console.WriteLine($"Default: {defaultValue}");
        }

        string input = "";
        Console.Write("> ");

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("\n[Cancelled]");
                throw new InputCancelledException();
            }

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();

                if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultValue))
                {
                    input = defaultValue;
                }

                if (input.Length > maxSize)
                {
                    AnsiConsole.MarkupLine($"[red]Input too large. Max size {maxSize} characters.[/]");
                    Console.Write("> ");
                    continue;
                }

                if (validator != null)
                {
                    var validationResult = validator(input);
                    if (!validationResult.Successful)
                    {
                        AnsiConsole.MarkupLine($"[red]{validationResult.Message}[/]");
                        Console.Write("> ");
                        continue;
                    }
                }

                return input;
            }

            if (keyInfo.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input = input.Substring(0, input.Length - 1);
                Console.Write("\b \b"); //  last character
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                input += keyInfo.KeyChar;
                Console.Write(keyInfo.KeyChar);
            }
        }
    }

    public static int AskForInt(
        string prompt,
        int? defaultValue = null,
        int? min = null,
        int? max = null)
    {
        while (true)
        {
            try
            {
                string input = AskForString(
                    prompt,
                    defaultValue?.ToString(),
                    value =>
                    {
                        if (!int.TryParse(value, out int result))
                        {
                            return ValidationResult.Error("Input must be a valid integer.");
                        }

                        if (min.HasValue && result < min.Value)
                        {
                            return ValidationResult.Error($"Value must be at least {min.Value}.");
                        }

                        if (max.HasValue && result > max.Value)
                        {
                            return ValidationResult.Error($"Value must be at most {max.Value}.");
                        }

                        return ValidationResult.Success();
                    });

                return int.Parse(input);
            }
            catch (InputCancelledException)
            {
                throw;
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid integer.[/]");
            }
        }
    }

    public static long AskForLong(
        string prompt,
        long? defaultValue = null,
        long? min = null,
        long? max = null)
    {
        while (true)
        {
            try
            {
                string input = AskForString(
                    prompt,
                    defaultValue?.ToString(),
                    value =>
                    {
                        if (!long.TryParse(value, out long result))
                        {
                            return ValidationResult.Error("Input must be a valid integer.");
                        }

                        if (min.HasValue && result < min.Value)
                        {
                            return ValidationResult.Error($"Value must be at least {min.Value}.");
                        }

                        if (max.HasValue && result > max.Value)
                        {
                            return ValidationResult.Error($"Value must be at most {max.Value}.");
                        }

                        return ValidationResult.Success();
                    });

                return long.Parse(input);
            }
            catch (InputCancelledException)
            {
                throw;
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid integer.[/]");
            }
        }
    }

    public static decimal AskForDecimal(
        string prompt,
        decimal? defaultValue = null,
        decimal? min = null,
        decimal? max = null)
    {
        while (true)
        {
            try
            {
                string input = AskForString(
                    prompt,
                    defaultValue?.ToString(),
                    value =>
                    {
                        if (!decimal.TryParse(value, out decimal result))
                        {
                            return ValidationResult.Error("Input must be a valid decimal number.");
                        }

                        if (min.HasValue && result < min.Value)
                        {
                            return ValidationResult.Error($"Value must be at least {min.Value}.");
                        }

                        if (max.HasValue && result > max.Value)
                        {
                            return ValidationResult.Error($"Value must be at most {max.Value}.");
                        }

                        return ValidationResult.Success();
                    });

                return decimal.Parse(input);
            }
            catch (InputCancelledException)
            {
                throw;
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid decimal number.[/]");
            }
        }
    }

    public static bool AskForConfirmation(string prompt, bool? defaultValue = null)
    {
        string defaultText = defaultValue.HasValue
            ? defaultValue.Value ? "(Y/n)" : "(y/N)"
            : "(y/n)";

        AnsiConsole.MarkupLine("[grey](Press [blue]Escape[/] to cancel and return)[/]");
        Console.WriteLine($"{prompt} {defaultText}");
        Console.Write("> ");

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("\n[Cancelled]");
                throw new InputCancelledException();
            }

            if (keyInfo.Key == ConsoleKey.Y || keyInfo.KeyChar == 'y')
            {
                Console.WriteLine("Yes");
                return true;
            }

            if (keyInfo.Key == ConsoleKey.N || keyInfo.KeyChar == 'n')
            {
                Console.WriteLine("No");
                return false;
            }

            if (keyInfo.Key == ConsoleKey.Enter && defaultValue.HasValue)
            {
                Console.WriteLine(defaultValue.Value ? "Yes" : "No");
                return defaultValue.Value;
            }
        }
    }

    public static DateTime AskForDate(
        string prompt,
        DateTime? defaultValue = null)
    {
        while (true)
        {
            try
            {
                string dateFormat = "yyyy-MM-dd";
                string input = AskForString(
                    prompt,
                    defaultValue?.ToString(dateFormat),
                    value =>
                    {
                        if (string.IsNullOrWhiteSpace(value) && defaultValue.HasValue)
                        {
                            return ValidationResult.Success();
                        }

                        if (!DateTime.TryParse(value, out _))
                        {
                            return ValidationResult.Error("Input must be a valid date (YYYY-MM-DD).");
                        }

                        return ValidationResult.Success();
                    });

                if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue)
                {
                    return defaultValue.Value;
                }

                return DateTime.Parse(input);
            }
            catch (InputCancelledException)
            {
                throw;
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Invalid date format. Please use YYYY-MM-DD format.[/]");
            }
        }
    }

    public static DateTime AskForDateTime(
        string prompt,
        DateTime? defaultValue = null)
    {
        while (true)
        {
            try
            {
                string dateTimeFormat = "yyyy-MM-dd HH:mm";
                string input = AskForString(
                    prompt,
                    defaultValue?.ToString(dateTimeFormat),
                    value =>
                    {
                        if (string.IsNullOrWhiteSpace(value) && defaultValue.HasValue)
                        {
                            return ValidationResult.Success();
                        }

                        if (!DateTime.TryParse(value, out _))
                        {
                            return ValidationResult.Error("Input must be a valid date and time (YYYY-MM-DD HH:MM).");
                        }

                        return ValidationResult.Success();
                    });

                if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue)
                {
                    return defaultValue.Value;
                }

                return DateTime.Parse(input);
            }
            catch (InputCancelledException)
            {
                throw;
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Invalid date/time format. Please use YYYY-MM-DD HH:MM format.[/]");
            }
        }
    }

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

        AnsiConsole.MarkupLine($"[bold]{title}[/]");
        AnsiConsole.MarkupLine("[grey](Use arrow keys to navigate, Enter to select, Escape to cancel)[/]");

        int visibleItems = Math.Min(pageSize, options.Count);
        int currentIndex = 0;
        int startIndex = 0;

        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[bold]{title}[/]");
            AnsiConsole.MarkupLine("[grey](Use arrow keys to navigate, Enter to select, Escape to cancel)[/]");

            for (int i = 0; i < visibleItems; i++)
            {
                int itemIndex = startIndex + i;
                if (itemIndex < options.Count)
                {
                    if (itemIndex == currentIndex)
                    {
                        AnsiConsole.MarkupLine($"[green]> {options[itemIndex]}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  {options[itemIndex]}");
                    }
                }
            }

            if (options.Count > visibleItems)
            {
                int currentPage = (currentIndex / visibleItems) + 1;
                int totalPages = (options.Count + visibleItems - 1) / visibleItems;
                AnsiConsole.MarkupLine($"[grey]Page {currentPage}/{totalPages}[/]");
            }

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    currentIndex = (currentIndex > 0) ? currentIndex - 1 : options.Count - 1;
                    if (currentIndex < startIndex)
                    {
                        startIndex = Math.Max(0, currentIndex - visibleItems + 1);
                    }
                    else if (currentIndex >= startIndex + visibleItems)
                    {
                        startIndex = currentIndex;
                    }
                    break;

                case ConsoleKey.DownArrow:
                    currentIndex = (currentIndex < options.Count - 1) ? currentIndex + 1 : 0;
                    if (currentIndex >= startIndex + visibleItems)
                    {
                        startIndex = Math.Max(0, currentIndex - visibleItems + 1);
                    }
                    else if (currentIndex < startIndex)
                    {
                        startIndex = currentIndex;
                    }
                    break;

                case ConsoleKey.Home:
                    currentIndex = 0;
                    startIndex = 0;
                    break;

                case ConsoleKey.End:
                    currentIndex = options.Count - 1;
                    startIndex = Math.Max(0, options.Count - visibleItems);
                    break;

                case ConsoleKey.PageUp:
                    currentIndex = Math.Max(0, currentIndex - visibleItems);
                    startIndex = Math.Max(0, startIndex - visibleItems);
                    break;

                case ConsoleKey.PageDown:
                    currentIndex = Math.Min(options.Count - 1, currentIndex + visibleItems);
                    startIndex = Math.Min(options.Count - visibleItems, startIndex + visibleItems);
                    if (startIndex < 0) startIndex = 0;
                    break;

                case ConsoleKey.Enter:
                    Console.Clear();
                    return itemList[currentIndex];

                case ConsoleKey.Escape:
                    Console.Clear();
                    AnsiConsole.MarkupLine("[yellow]Selection cancelled.[/]");
                    throw new InputCancelledException();
            }
        }
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

        AnsiConsole.MarkupLine($"[bold]{title}[/]");
        AnsiConsole.MarkupLine("[grey](Use arrow keys to navigate, Enter to select, Escape to cancel)[/]");

        int visibleItems = Math.Min(pageSize, options.Count);
        int currentIndex = 0;
        int startIndex = 0;

        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[bold]{title}[/]");
            AnsiConsole.MarkupLine("[grey](Use arrow keys to navigate, Enter to select, Escape to cancel)[/]");

            for (int i = 0; i < visibleItems; i++)
            {
                int itemIndex = startIndex + i;
                if (itemIndex < options.Count)
                {
                    if (itemIndex == currentIndex)
                    {
                        AnsiConsole.MarkupLine($"[green]> {options[itemIndex]}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  {options[itemIndex]}");
                    }
                }
            }

            if (options.Count > visibleItems)
            {
                int currentPage = (currentIndex / visibleItems) + 1;
                int totalPages = (options.Count + visibleItems - 1) / visibleItems;
                AnsiConsole.MarkupLine($"[grey]Page {currentPage}/{totalPages}[/]");
            }

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    currentIndex = (currentIndex > 0) ? currentIndex - 1 : options.Count - 1;
                    if (currentIndex < startIndex)
                    {
                        startIndex = Math.Max(0, currentIndex - visibleItems + 1);
                    }
                    else if (currentIndex >= startIndex + visibleItems)
                    {
                        startIndex = currentIndex;
                    }
                    break;

                case ConsoleKey.DownArrow:
                    currentIndex = (currentIndex < options.Count - 1) ? currentIndex + 1 : 0;
                    if (currentIndex >= startIndex + visibleItems)
                    {
                        startIndex = Math.Max(0, currentIndex - visibleItems + 1);
                    }
                    else if (currentIndex < startIndex)
                    {
                        startIndex = currentIndex;
                    }
                    break;

                case ConsoleKey.Home:
                    currentIndex = 0;
                    startIndex = 0;
                    break;

                case ConsoleKey.End:
                    currentIndex = options.Count - 1;
                    startIndex = Math.Max(0, options.Count - visibleItems);
                    break;

                case ConsoleKey.PageUp:
                    currentIndex = Math.Max(0, currentIndex - visibleItems);
                    startIndex = Math.Max(0, startIndex - visibleItems);
                    break;

                case ConsoleKey.PageDown:
                    currentIndex = Math.Min(options.Count - 1, currentIndex + visibleItems);
                    startIndex = Math.Min(options.Count - visibleItems, startIndex + visibleItems);
                    if (startIndex < 0) startIndex = 0;
                    break;

                case ConsoleKey.Enter:
                    Console.Clear();
                    string idPart = options[currentIndex].Split(':')[0].Trim();
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

                case ConsoleKey.Escape:
                    Console.Clear();
                    AnsiConsole.MarkupLine("[yellow]Selection cancelled.[/]");
                    throw new InputCancelledException();
            }
        }
    }

    public static List<string> MultiSelectFromList(
        IEnumerable<string> items,
        string title,
        string instructionsText = "[grey](Use arrow keys to navigate, Space to select, Enter to confirm, Escape to cancel)[/]",
        int pageSize = 10)
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No items available for selection.[/]");
            return new List<string>();
        }

        AnsiConsole.MarkupLine($"[bold]{title}[/]");
        AnsiConsole.MarkupLine(instructionsText);

        int visibleItems = Math.Min(pageSize, itemList.Count);
        int currentIndex = 0;
        int startIndex = 0;
        var selectedItems = new HashSet<int>();

        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[bold]{title}[/]");
            AnsiConsole.MarkupLine(instructionsText);

            for (int i = 0; i < visibleItems; i++)
            {
                int itemIndex = startIndex + i;
                if (itemIndex < itemList.Count)
                {
                    string selectionMark = selectedItems.Contains(itemIndex) ? "[x]" : "[ ]";

                    if (itemIndex == currentIndex)
                    {
                        AnsiConsole.MarkupLine($"[green]> {selectionMark} {itemList[itemIndex]}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  {selectionMark} {itemList[itemIndex]}");
                    }
                }
            }

            if (itemList.Count > visibleItems)
            {
                int currentPage = (currentIndex / visibleItems) + 1;
                int totalPages = (itemList.Count + visibleItems - 1) / visibleItems;
                AnsiConsole.MarkupLine($"[grey]Page {currentPage}/{totalPages}[/]");
            }

            AnsiConsole.MarkupLine($"[grey]Selected: {selectedItems.Count} item(s)[/]");

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    currentIndex = (currentIndex > 0) ? currentIndex - 1 : itemList.Count - 1;
                    if (currentIndex < startIndex)
                    {
                        startIndex = Math.Max(0, currentIndex - visibleItems + 1);
                    }
                    else if (currentIndex >= startIndex + visibleItems)
                    {
                        startIndex = currentIndex;
                    }
                    break;

                case ConsoleKey.DownArrow:
                    currentIndex = (currentIndex < itemList.Count - 1) ? currentIndex + 1 : 0;
                    if (currentIndex >= startIndex + visibleItems)
                    {
                        startIndex = Math.Max(0, currentIndex - visibleItems + 1);
                    }
                    else if (currentIndex < startIndex)
                    {
                        startIndex = currentIndex;
                    }
                    break;

                case ConsoleKey.Spacebar:
                    if (selectedItems.Contains(currentIndex))
                    {
                        selectedItems.Remove(currentIndex);
                    }
                    else
                    {
                        selectedItems.Add(currentIndex);
                    }
                    break;

                case ConsoleKey.Home:
                    currentIndex = 0;
                    startIndex = 0;
                    break;

                case ConsoleKey.End:
                    currentIndex = itemList.Count - 1;
                    startIndex = Math.Max(0, itemList.Count - visibleItems);
                    break;

                case ConsoleKey.PageUp:
                    currentIndex = Math.Max(0, currentIndex - visibleItems);
                    startIndex = Math.Max(0, startIndex - visibleItems);
                    break;

                case ConsoleKey.PageDown:
                    currentIndex = Math.Min(itemList.Count - 1, currentIndex + visibleItems);
                    startIndex = Math.Min(itemList.Count - visibleItems, startIndex + visibleItems);
                    if (startIndex < 0) startIndex = 0;
                    break;

                case ConsoleKey.A:
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        selectedItems.Add(i);
                    }
                    break;

                case ConsoleKey.N:
                    selectedItems.Clear();
                    break;

                case ConsoleKey.Enter:
                    Console.Clear();
                    return selectedItems.Select(index => itemList[index]).ToList();

                case ConsoleKey.Escape:
                    Console.Clear();
                    AnsiConsole.MarkupLine("[yellow]Selection cancelled.[/]");
                    throw new InputCancelledException();
            }
        }
    }
}