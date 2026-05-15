# Watt Authentication Quick Start

## What This Uses

Watt authentication is Azure CLI based.

Watt stores only environment metadata in watt_environments.json and fetches Dataverse access tokens through AzureCliCredential at connection time.

## One Minute Setup

1. Sign in to Azure CLI
- Run az login and complete sign-in.

2. Add a Dataverse environment
- Run dotnet run --project Watt/Watt.csproj -- env add MyEnv https://org.crm.dynamics.com

3. Select active environment
- Run dotnet run --project Watt/Watt.csproj -- env select MyEnv

4. Launch TUI
- Run dotnet run --project Watt/Watt.csproj

## CLI Commands

- Add environment:
dotnet run --project Watt/Watt.csproj -- env add <name> <url>

- List environments:
dotnet run --project Watt/Watt.csproj -- env list

- Select active environment:
dotnet run --project Watt/Watt.csproj -- env select <name|id>

- Remove environment:
dotnet run --project Watt/Watt.csproj -- env remove <name|id>

## How It Works

1. CredentialManager loads environments from disk.
2. Program resolves the active environment.
3. DataverseConnectionManager requests token via AzureCliCredential.
4. ServiceClient is created and attached to AppState.
5. Tool views use AppState.ServiceClient.

## File Map

Authentication files:
- Watt/Core/Authentication/CredentialManager.cs
- Watt/Core/Authentication/DataverseConnectionManager.cs
- Watt/Core/Authentication/Models/EnvironmentDetails.cs

Startup and CLI:
- Watt/Program.cs
- Watt/CLI/CliHandler.cs

## Troubleshooting Checklist

- Azure CLI login is valid
- Environment URL starts with https://
- Active environment is selected
- Dataverse user has permission
- Environment appears in env list output

## Important Differences From Older Docs

This quick start intentionally does not include:
- OAuthAuthenticationProvider setup
- Client secret auth flow
- Username password auth flow
- Stored credentials file

Those flows are not present in the current source implementation.

