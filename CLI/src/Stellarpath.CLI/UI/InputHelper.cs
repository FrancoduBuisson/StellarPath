using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class InputHelper
{
    public static string AskForString(
        string prompt,
        string defaultValue = null,
        Func<string, ValidationResult> validator = null)
    {
        var textPrompt = new TextPrompt<string>(prompt);

        if (!string.IsNullOrEmpty(defaultValue))
        {
            textPrompt = textPrompt.DefaultValue(defaultValue);
            prompt = $"{prompt} (default: {defaultValue})";
        }

        if (validator != null)
        {
            textPrompt = textPrompt.Validate(validator);
        }

        return AnsiConsole.Prompt(textPrompt);
    }
    public static int AskForInt(
        string prompt,
        int? defaultValue = null,
        int? min = null,
        int? max = null)
    {
        var textPrompt = new TextPrompt<int>(prompt);

        if (defaultValue.HasValue)
        {
            textPrompt = textPrompt.DefaultValue(defaultValue.Value);
            prompt = $"{prompt} (default: {defaultValue})";
        }

        if (min.HasValue)
        {
            textPrompt = textPrompt.Validate(value =>
                value >= min.Value
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be at least {min.Value}"));
        }

        if (max.HasValue)
        {
            textPrompt = textPrompt.Validate(value =>
                value <= max.Value
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be at most {max.Value}"));
        }

        return AnsiConsole.Prompt(textPrompt);
    }

    public static long AskForLong(
        string prompt,
        long? defaultValue = null,
        long? min = null,
        long? max = null)
    {
        var textPrompt = new TextPrompt<long>(prompt);

        if (defaultValue.HasValue)
        {
            textPrompt = textPrompt.DefaultValue(defaultValue.Value);
            prompt = $"{prompt} (default: {defaultValue})";
        }

        if (min.HasValue)
        {
            textPrompt = textPrompt.Validate(value =>
                value >= min.Value
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be at least {min.Value}"));
        }

        if (max.HasValue)
        {
            textPrompt = textPrompt.Validate(value =>
                value <= max.Value
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be at most {max.Value}"));
        }

        return AnsiConsole.Prompt(textPrompt);
    }

    public static decimal AskForDecimal(
        string prompt,
        decimal? defaultValue = null,
        decimal? min = null,
        decimal? max = null)
    {
        var textPrompt = new TextPrompt<decimal>(prompt);

        if (defaultValue.HasValue)
        {
            textPrompt = textPrompt.DefaultValue(defaultValue.Value);
            prompt = $"{prompt} (default: {defaultValue})";
        }

        if (min.HasValue)
        {
            textPrompt = textPrompt.Validate(value =>
                value >= min.Value
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be at least {min.Value}"));
        }

        if (max.HasValue)
        {
            textPrompt = textPrompt.Validate(value =>
                value <= max.Value
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Value must be at most {max.Value}"));
        }

        return AnsiConsole.Prompt(textPrompt);
    }

    public static bool AskForConfirmation(string prompt, bool? defaultValue = null)
    {
        return AnsiConsole.Confirm(prompt, defaultValue ?? false);
    }

    public static DateTime AskForDate(
        string prompt,
        DateTime? defaultValue = null)
    {
        string datePrompt = prompt;
        if (defaultValue.HasValue)
        {
            datePrompt += $" (default: {defaultValue.Value:yyyy-MM-dd})";
        }

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>(datePrompt)
                    .DefaultValue(defaultValue.HasValue ? defaultValue.Value.ToString("yyyy-MM-dd") : "")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            if (DateTime.TryParse(input, out var result))
            {
                return result;
            }

            AnsiConsole.MarkupLine("[red]Invalid date format. Please use YYYY-MM-DD format.[/]");
        }
    }

    public static DateTime AskForDateTime(
        string prompt,
        DateTime? defaultValue = null)
    {
        string dateTimePrompt = prompt;
        if (defaultValue.HasValue)
        {
            dateTimePrompt += $" (default: {defaultValue.Value:yyyy-MM-dd HH:mm})";
        }

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>(dateTimePrompt)
                    .DefaultValue(defaultValue.HasValue ? defaultValue.Value.ToString("yyyy-MM-dd HH:mm") : "")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            if (DateTime.TryParse(input, out var result))
            {
                return result;
            }

            AnsiConsole.MarkupLine("[red]Invalid date/time format. Please use YYYY-MM-DD HH:MM format.[/]");
        }
    }

    public static bool CollectSearchCriteria<T>(string criteriaName, Action<T> action, T criteria) where T : class
    {
        var include = AnsiConsole.Confirm($"Do you want to search by {criteriaName}?", false);
        if (include)
        {
            action(criteria);
        }
        return include;
    }
}