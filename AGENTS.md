# Watt — Agent Guidelines

Watt is a .NET 10 CLI + TUI application for Power Platform / Dataverse developers. It is written in C# with Terminal.Gui for the TUI layer and Spectre.Console for CLI output.

## Project structure

```
Watt/
  Program.cs              # Entry point — routes to CLI mode or TUI mode
  CLI/
    CliHandler.cs         # Parses and dispatches CLI subcommands (watt env …, watt tool …)
  Core/
    AppState.cs           # Shared runtime state passed to every tool
    IToolView.cs          # Interface every TUI tool must implement
    HelpDialog.cs
    Authentication/       # Azure CLI-backed auth; no credentials stored by Watt itself
  Tools/
    DRF/                  # Duplicate Row Finder logic (pure business logic, no UI)
  UI/
    ToolSelector.cs
    Tools/
      DRFView.cs          # TUI view for Duplicate Row Finder
      InspectorView.cs    # TUI view for Table Metadata Inspector
```

## Architecture rules

- **CLI vs TUI split**: `Program.cs` checks `args.Length > 0` to decide mode. CLI commands must be fully self-contained and exit with an integer code; they must not touch Terminal.Gui types.
- **Tool separation**: Business logic lives in `Tools/<ToolName>/`, UI lives in `UI/Tools/`. A `*View` class implements `IToolView` and holds a reference to its corresponding tool class. Keep them decoupled.
- **AppState**: The single source of truth passed top-down. Do not use statics or singletons outside of `AppState`. Add new shared services as required properties on `AppState`.
- **Authentication**: All credential acquisition goes through `AuthenticationService`, which delegates to `AzureCliAuthenticationProvider`. Watt never stores secrets — environment metadata (name, org URL, ID) is stored, tokens are always fetched live from the Azure CLI.

## Adding a new tool

1. Create `Tools/<ToolName>/<ToolName>Tool.cs` for the business logic.
2. Create `UI/Tools/<ToolName>View.cs` implementing `IToolView`.
3. Register the new view in the `tools` list in `Program.cs`.
4. If the tool needs a CLI entry point, add a handler in `CliHandler.cs` under the `tool` subcommand.

## Build & run

```bash
# Build
dotnet build Watt/Watt.csproj

# Run TUI (requires an active environment set first)
dotnet run --project Watt/Watt.csproj

# CLI — add and activate an environment
dotnet run --project Watt/Watt.csproj -- env add <name> <orgUrl>
dotnet run --project Watt/Watt.csproj -- env set <name>
```

## Coding conventions

- Target **C# 13 / .NET 10**. Use nullable reference types (`#nullable enable` is project-wide).
- Use `async/await` throughout; avoid `.Result` or `.Wait()` except where Terminal.Gui forces it.
- Prefer collection expressions (`[.. ]`) and primary constructors where they improve readability.
- XML doc comments on all `public` and `internal` types and members.
- Do not add `using` directives for namespaces that are already covered by implicit global usings.

## Dependencies

| Package | Purpose |
|---|---|
| `Terminal.Gui` | TUI framework |
| `Spectre.Console` | CLI output formatting |
| `Azure.Identity` | Token acquisition via Azure CLI |
| `Microsoft.PowerPlatform.Dataverse.Client` | Dataverse SDK (`ServiceClient`) |

Do not introduce new NuGet packages without a clear reason. Prefer inbox .NET APIs where they suffice.

## Out of scope

- Do not store, log, or transmit credentials or tokens.
- Do not add telemetry.
- Do not target anything below .NET 10.
