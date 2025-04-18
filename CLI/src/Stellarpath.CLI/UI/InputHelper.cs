using Spectre.Console;

namespace Stellarpath.CLI.UI;

public static class InputHelper
{
    public static string AskForString(
        string prompt,
        string defaultValue = null,
        Func<string, ValidationResult> validator = null,
        int maxSize = 100)
    {
        return EscapableInputHelper.AskForString(prompt, defaultValue, validator, maxSize);
    }

    public static int AskForInt(
        string prompt,
        int? defaultValue = null,
        int? min = null,
        int? max = null)
    {
        return EscapableInputHelper.AskForInt(prompt, defaultValue, min, max);
    }

    public static long AskForLong(
        string prompt,
        long? defaultValue = null,
        long? min = null,
        long? max = null)
    {
        return EscapableInputHelper.AskForLong(prompt, defaultValue, min, max);
    }

    public static decimal AskForDecimal(
        string prompt,
        decimal? defaultValue = null,
        decimal? min = null,
        decimal? max = null)
    {
        return EscapableInputHelper.AskForDecimal(prompt, defaultValue, min, max);
    }

    public static bool AskForConfirmation(string prompt, bool? defaultValue = null)
    {
        return EscapableInputHelper.AskForConfirmation(prompt, defaultValue);
    }

    public static DateTime AskForDate(
        string prompt,
        DateTime? defaultValue = null)
    {
        return EscapableInputHelper.AskForDate(prompt, defaultValue);
    }

    public static DateTime AskForDateTime(
        string prompt,
        DateTime? defaultValue = null)
    {
        return EscapableInputHelper.AskForDateTime(prompt, defaultValue);
    }

    public static bool CollectSearchCriteria<T>(string criteriaName, Action<T> action, T criteria) where T : class
    {
        try
        {
            var include = EscapableInputHelper.AskForConfirmation($"Do you want to search by {criteriaName}?", false);
            if (include)
            {
                action(criteria);
            }
            return include;
        }
        catch (EscapableInputHelper.InputCancelledException)
        {
            throw;
        }
    }
}