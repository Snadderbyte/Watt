using System;
using System.Linq;
using System.Threading.Tasks;
using Watt.Core.Authentication;

namespace Watt.CLI;

/// <summary>
/// Handles CLI subcommands before the TUI is launched.
/// Usage: watt env add &lt;name&gt; &lt;url&gt;
///        watt env list
///        watt env remove &lt;name|id&gt;
/// </summary>
internal static class CliHandler
{
    internal static async Task<int> RunAsync(string[] args, AuthenticationService authService)
    {
        if (args.Length == 0 || args[0] != "env")
        {
            Console.Error.WriteLine($"Unknown command: {args[0]}");
            PrintUsage();
            return 1;
        }

        return await HandleEnvCommandAsync(args[1..], authService);
    }

    private static async Task<int> HandleEnvCommandAsync(string[] subArgs, AuthenticationService authService)
    {
        if (subArgs.Length == 0)
        {
            Console.Error.WriteLine("Usage: watt env <add|list|remove>");
            return 1;
        }

        return subArgs[0] switch
        {
            "add"    => await HandleEnvAddAsync(subArgs[1..], authService),
            "list"   => HandleEnvList(authService),
            "remove" => await HandleEnvRemoveAsync(subArgs[1..], authService),
            _        => HandleUnknown(subArgs[0])
        };
    }

    private static async Task<int> HandleEnvAddAsync(string[] args, AuthenticationService authService)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: watt env add <name> <url>");
            return 1;
        }

        var name = args[0];
        var url  = args[1];

        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Error: URL must start with https://");
            return 1;
        }

        var environment = new EnvironmentDetails
        {
            Id     = Guid.NewGuid().ToString(),
            Name   = name,
            OrgUrl = url
        };

        await authService.RegisterEnvironmentAsync(environment);
        Console.WriteLine($"Environment '{name}' added ({url})");
        return 0;
    }

    private static int HandleEnvList(AuthenticationService authService)
    {
        var environments = authService.GetAllEnvironments().ToList();

        if (environments.Count == 0)
        {
            Console.WriteLine("No environments registered. Use 'watt env add <name> <url>' to add one.");
            return 0;
        }

        foreach (var env in environments)
            Console.WriteLine($"{env.Name,-30} {env.OrgUrl,-60} {env.Id}");

        return 0;
    }

    private static async Task<int> HandleEnvRemoveAsync(string[] args, AuthenticationService authService)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: watt env remove <name|id>");
            return 1;
        }

        var nameOrId     = args[0];
        var environments = authService.GetAllEnvironments().ToList();
        var env = environments.FirstOrDefault(e =>
            e.Id.Equals(nameOrId,   StringComparison.OrdinalIgnoreCase) ||
            e.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase));

        if (env == null)
        {
            Console.Error.WriteLine($"Environment '{nameOrId}' not found.");
            return 1;
        }

        await authService.DeleteEnvironmentAsync(env.Id);
        Console.WriteLine($"Environment '{env.Name}' removed.");
        return 0;
    }

    private static int HandleUnknown(string subcommand)
    {
        Console.Error.WriteLine($"Unknown subcommand: {subcommand}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: watt [command]");
        Console.WriteLine("  (no args)                     Launch the TUI");
        Console.WriteLine("  env add <name> <url>          Register a Dataverse environment");
        Console.WriteLine("  env list                      List all registered environments");
        Console.WriteLine("  env remove <name|id>          Remove a registered environment");
    }
}
