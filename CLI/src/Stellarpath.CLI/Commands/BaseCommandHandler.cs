using Spectre.Console;
using Stellarpath.CLI.Core;

namespace Stellarpath.CLI.Commands;

public abstract class CommandHandlerBase<T> where T : class
{
    protected readonly CommandContext Context;

    protected CommandHandlerBase(CommandContext context)
    {
        Context = context;
    }

    public async Task HandleAsync()
    {
        var options = GetBaseOptions();

        var entityOptions = GetEntitySpecificOptions();
        options.AddRange(entityOptions);

        if (Context.CurrentUser?.Role == "Admin")
        {
            var adminOptions = GetAdminOptions();
            options.AddRange(adminOptions);
        }

        options.Add("Back to Main Menu");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(GetMenuTitle())
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(options));

        await ProcessSelectionAsync(selection);
    }

    protected abstract string GetMenuTitle();

    protected virtual List<string> GetBaseOptions()
    {
        return new List<string>
        {
            $"List All {GetEntityNamePlural()}",
            $"List Active {GetEntityNamePlural()}",
            $"View {GetEntityName()} Details",
            $"Search {GetEntityNamePlural()}"
        };
    }

    protected virtual List<string> GetEntitySpecificOptions()
    {
        return new List<string>();
    }

    protected virtual List<string> GetAdminOptions()
    {
        return new List<string>
        {
            $"Create New {GetEntityName()}",
            $"Update {GetEntityName()}",
            $"Activate {GetEntityName()}",
            $"Deactivate {GetEntityName()}"
        };
    }

    protected abstract Task ProcessSelectionAsync(string selection);

    protected abstract string GetEntityName();

    protected abstract string GetEntityNamePlural();

    protected abstract void DisplayEntities(IEnumerable<T> entities, string title);

    protected abstract void DisplayEntityDetails(T entity);

    protected bool EnsureAdminPermission()
    {
        if (Context.CurrentUser?.Role != "Admin")
        {
            AnsiConsole.MarkupLine($"[red]You don't have permission to perform this action on {GetEntityNamePlural().ToLower()}.[/]");
            return false;
        }
        return true;
    }

    protected async Task<TResult> ExecuteWithSpinnerAsync<TResult>(
        string statusMessage,
        Func<StatusContext, Task<TResult>> action)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .SpinnerStyle("green")
            .StartAsync(statusMessage, action);
    }

    protected async Task<TEntity> FetchAndPromptForEntitySelectionAsync<TService, TEntity>(
        TService service,
        Func<TService, Task<IEnumerable<TEntity>>> fetchMethod,
        Func<TEntity, string> displaySelector,
        Func<TEntity, int> idSelector,
        string fetchingMessage,
        string notFoundMessage,
        string selectionPromptTitle) where TService : class where TEntity : class
    {
        await ExecuteWithSpinnerAsync(fetchingMessage, async ctx =>
        {
            var entities = await fetchMethod(service);

            if (!entities.Any())
            {
                AnsiConsole.MarkupLine($"[yellow]{notFoundMessage}[/]");
                return false;
            }

            ctx.Status("Select an entity");
            return true;
        });

        var entities = await fetchMethod(service);
        if (!entities.Any())
        {
            return null;
        }

        var entityOptions = entities.Select(e => $"{idSelector(e)}: {displaySelector(e)}").ToList();
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(selectionPromptTitle)
                .PageSize(10)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(entityOptions));

        int selectedId = int.Parse(selectedOption.Split(':')[0].Trim());
        return entities.FirstOrDefault(e => idSelector(e) == selectedId);
    }
}