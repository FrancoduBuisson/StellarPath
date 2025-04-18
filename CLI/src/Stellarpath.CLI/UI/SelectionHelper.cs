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
        return EscapableInputHelper.SelectFromList(items, displaySelector, title, pageSize);
    }

    public static T SelectFromListById<T, TId>(
        IEnumerable<T> items,
        Func<T, TId> idSelector,
        Func<T, string> displaySelector,
        string title,
        int pageSize = 10) where T : class
    {
        return EscapableInputHelper.SelectFromListById(items, idSelector, displaySelector, title, pageSize);
    }
}