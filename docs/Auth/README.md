# Watt Authentication System

## Overview

Watt uses Azure CLI based authentication for Dataverse access.

The app stores only environment metadata:
- Environment id
- Environment name
- Dataverse org URL
- Active environment flag

Watt does not store credentials, client secrets, passwords, refresh tokens, or access tokens.

Authentication tokens are requested on demand through Azure.Identity AzureCliCredential when a Dataverse connection is created.

## Current Architecture

### Component: EnvironmentDetails

EnvironmentDetails is the persisted model for each Dataverse environment.

Fields:
- Id
- Name
- IsActive
- OrgUrl

Source: Watt/Core/Authentication/Models/EnvironmentDetails.cs

### Component: CredentialManager

CredentialManager manages environment metadata lifecycle and persistence.

Responsibilities:
- Load environments from disk at startup
- Save new environment entries
- Mark one environment as active
- Remove environments
- Expose all environments for CLI listing and selection

Persistence:
- File name: watt_environments.json
- Folder: AppData/Watt for current user

Source: Watt/Core/Authentication/CredentialManager.cs

### Component: DataverseConnectionManager

DataverseConnectionManager creates ServiceClient instances for environments.

Responsibilities:
- Resolve environment metadata by id
- Build Dataverse scope as orgUrl/.default
- Request token via AzureCliCredential
- Construct ServiceClient using token callback
- Cache active ServiceClient instances in memory
- Dispose one or all connections

Source: Watt/Core/Authentication/DataverseConnectionManager.cs

### Component: Program startup wiring

Program composes the authentication flow.

CLI mode:
- Load environment metadata
- Execute env commands
- Dispose manager

TUI mode:
- Load environment metadata
- Require one active environment
- Create DataverseConnectionManager
- Create initial ServiceClient for active environment
- Store connection objects in AppState

Source: Watt/Program.cs

### Component: AppState integration

AppState exposes:
- ServiceClient
- DataverseConnectionManager
- SelectedTool

Source: Watt/Core/AppState.cs

## CLI Environment Management

The CLI uses env subcommands to manage environment metadata:

- env add <name> <url>
- env list
- env select <name|id>
- env remove <name|id>

Source: Watt/CLI/CliHandler.cs

Note: Current implementation uses select, not set.

## Runtime Flow

1. User runs env add to register one or more Dataverse org URLs.
2. User runs env select to mark one environment active.
3. User launches TUI without arguments.
4. Program resolves active environment and opens initial ServiceClient.
5. Tools consume the shared ServiceClient through AppState.
6. User can refresh connection from the status shortcut.

## Security Model

1. No stored secrets
- Watt stores only non-secret environment metadata.

2. Token acquisition from Azure CLI
- Tokens come from AzureCliCredential and are fetched when needed.

3. In-memory connection cache
- ServiceClient instances are in memory only and disposed on shutdown.

4. HTTPS Dataverse endpoints
- Org URL must be HTTPS in CLI add path.

## Prerequisites

1. Azure CLI installed
2. User authenticated with Azure CLI
3. Dataverse environment URL in HTTPS form, for example https://org.crm.dynamics.com
4. Account has Dataverse permissions

## Troubleshooting

### No active environment found in TUI

Cause: No environment is marked active.

Fix: Use env select with an existing environment name or id.

### Connection returns null

Common causes:
- Environment id not found
- Invalid org URL
- Azure CLI session not authenticated
- Dataverse permission issues

### URL rejected during env add

Cause: URL does not start with https://

### Environment list is empty

Cause: No environments registered yet.

Fix: Use env add first.

## Future Documentation Notes

If you introduce an AuthenticationService abstraction later, document it only after the concrete type exists in source.

