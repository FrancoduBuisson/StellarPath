using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand
        {
            new Option<int>("--number", "An integer option"),
            new Option<bool>("--flag", "A boolean option"),
            new Argument<string>("input", "A required input argument")
        };

        rootCommand.Description = "A simple CLI app";
        rootCommand.Handler = CommandHandler.Create<int, bool, string>((number, flag, input) =>
        {
            Console.WriteLine($"Number: {number}");
            Console.WriteLine($"Flag: {flag}");
            Console.WriteLine($"Input: {input}");
        });

        return rootCommand.Invoke(args);
    }
}