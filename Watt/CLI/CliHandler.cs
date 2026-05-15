using System;
using System.Linq;
using System.Threading.Tasks;
using Watt.Core.Authentication;
using Spectre.Console;
using Watt.Core;

namespace Watt.CLI;

/// <summary>
/// Handles CLI subcommands before the TUI is launched.
/// Usage: watt env add &lt;name&gt; &lt;url&gt;
///        watt env list
///        watt env remove &lt;name|id&gt;
/// </summary>
internal static class CliHandler
{
    internal static async Task<int> RunAsync(string[] args, CredentialManager credentialManager)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        if (args[0] == "env")
            return await HandleEnvCommandAsync(args[1..], credentialManager);

        if (args[0] == "tool")
            return await HandleToolCommandAsync(args[1..], null!); // Pass necessary services

        Console.Error.WriteLine($"Unknown command: {args[0]}");
        PrintUsage();
        return 1;
    }

    private static async Task<int> HandleToolCommandAsync(string[] subArgs, AppState appState)
    {
        if (subArgs.Length == 0)
        {
            Console.Error.WriteLine("Usage: watt tool <toolName> [options]");
            return 1;
        }

        return subArgs[0] switch
        {
            "drf" => await HandleToolSelectionAsync(subArgs[1..], appState),
            _ => HandleToolUnknown(subArgs[0])
        };
    }

    private static async Task<int> HandleEnvCommandAsync(string[] subArgs, CredentialManager credentialManager)
    {
        if (subArgs.Length == 0)
        {
            Console.Error.WriteLine("Usage: watt env <add|list|select|remove>");
            return 1;
        }

        return subArgs[0] switch
        {
            "add" => await HandleEnvAddAsync(subArgs[1..], credentialManager),
            "list" => HandleEnvList(credentialManager),
            "remove" => await HandleEnvRemoveAsync(subArgs[1..], credentialManager),
            "select" => await HandleEnvSelectAsync(subArgs[1..], credentialManager),
            _ => HandleEnvUnknown(subArgs[0])
        };
    }

    private static async Task<int> HandleToolSelectionAsync(string[] subArgs, AppState appState)
    {
        if (subArgs.Length == 0)
        {
            Console.Error.WriteLine("Usage: watt tool <toolName> [options]");
            return 1;
        }

        var toolName = subArgs[0].ToLower();
        switch (toolName)
        {
            case "drf":
                Console.WriteLine("Launching Duplicate Row Finder...");
                return 0;
            default:
                Console.Error.WriteLine($"Unknown tool: {toolName}");
                return 1;
        }
    }

    private static async Task<int> HandleEnvAddAsync(string[] args, CredentialManager credentialManager)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: watt env add <name> <url>");
            return 1;
        }

        var name = args[0];
        var url = args[1];

        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Error: URL must start with https://");
            return 1;
        }

        var environment = new EnvironmentDetails
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            IsActive = false,
            OrgUrl = url
        };

        await credentialManager.SaveEnvironmentAsync(environment);
        Console.WriteLine($"Environment '{name}' added ({url})");
        return 0;
    }
    private static int HandleEnvList(CredentialManager credentialManager)
    {
        var environments = credentialManager.GetAllEnvironments().ToList();

        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Active");
        table.AddColumn("Name");
        table.AddColumn("URL");
        table.AddColumn("ID");

        if (environments.Count == 0)
        {
            Console.WriteLine("No environments registered. Use 'watt env add <name> <url>' to add one.");
            return 0;
        }

        foreach (var env in environments)
        {
            table.AddRow(
                (table.Rows.Count + 1).ToString(),
                env.IsActive ? "[green]Yes[/]" : "No",
                env.Name,
                env.OrgUrl,
                env.Id);
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private static async Task<int> HandleEnvSelectAsync(string[] args, CredentialManager credentialManager)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: watt env select <name|id>");
            return 1;
        }

        var nameOrId = args[0];
        var environments = credentialManager.GetAllEnvironments().ToList();
        var env = environments.FirstOrDefault(e =>
            e.Id.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) ||
            e.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase));

        if (env == null)
        {
            Console.Error.WriteLine($"Environment '{nameOrId}' not found.");
            return 1;
        }

        await credentialManager.SetActiveEnvironmentAsync(env.Id);
        Console.WriteLine($"Environment '{env.Name}' selected.");
        return 0;
    }

    private static async Task<int> HandleEnvRemoveAsync(string[] args, CredentialManager credentialManager)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: watt env remove <name|id>");
            return 1;
        }

        var nameOrId = args[0];
        var environments = credentialManager.GetAllEnvironments().ToList();
        var env = environments.FirstOrDefault(e =>
            e.Id.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) ||
            e.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase));

        if (env == null)
        {
            Console.Error.WriteLine($"Environment '{nameOrId}' not found.");
            return 1;
        }

        await credentialManager.DeleteEnvironmentAsync(env.Id);
        Console.WriteLine($"Environment '{env.Name}' removed.");
        return 0;
    }

    private static int HandleEnvUnknown(string subcommand)
    {
        Console.Error.WriteLine($"Unknown subcommand: {subcommand}");
        PrintEnvUsage();
        return 1;
    }

    private static int HandleToolUnknown(string subcommand)
    {
        Console.Error.WriteLine($"Unknown subcommand: {subcommand}");
        PrintToolUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: watt [command]");
        Console.WriteLine("  (no args)                     Launch the TUI");
        Console.WriteLine("  env                           Manage environments");
        Console.WriteLine("  tool                          Select and run a tool");
    }

    private static void PrintEnvUsage()
    {
        Console.WriteLine("Usage: watt env <subcommand>");
        Console.WriteLine("  add <name> <url>              Add a new environment");
        Console.WriteLine("  list                          List all environments");
        Console.WriteLine("  select <name|id>              Select an active environment");
        Console.WriteLine("  remove <name|id>              Remove an environment");
    }

    private static void PrintToolUsage()
    {
    }
}
