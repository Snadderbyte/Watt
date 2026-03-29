# Watt Authentication - Quick Start Guide

## What Was Added

A complete, production-ready authentication system for Dataverse with:
- ✅ OAuth authentication (interactive login)
- ✅ Client Secret authentication (service principal)
- ✅ Username/Password authentication
- ✅ Secure credential storage (DPAPI encryption)
- ✅ Multiple environment support
- ✅ Connection pooling and management
- ✅ Terminal.Gui UI dialogs
- ✅ Automatic token validation

## One-Minute Setup

1. **Update OAuth Client ID** (if using OAuth)
   - Open `Core/Authentication/OAuthAuthenticationProvider.cs`
   - Update `ClientId` constant with your Azure AD app ID

2. **Build the project**
   ```powershell
   dotnet build
   ```

3. **Run the application**
   ```powershell
   dotnet run
   ```

4. **Click "Select Environment"** and add your first environment

## Quick Examples

### Example 1: OAuth Authentication (Interactive Login)

This is the simplest method for users to authenticate:

```csharp
// User clicks "Select Environment" → "Add"
// Enters environment name: "Production"
// Enters URL: "https://myorg.crm.dynamics.com"
// Selects OAuth
// Clicks "Next" and signs in with their Microsoft account
// Application now has access to that environment
```

### Example 2: Using Service Principal (Client Secret)

For automated/background processes:

```csharp
// Service principal details:
// Tenant ID:     "12345678-1234-1234-1234-123456789abc"
// Client ID:     "87654321-4321-4321-4321-cba987654321"
// Client Secret: "YourSecretHere~"

// Application stores these securely (encrypted)
// Create queries/operations that run unattended
```

### Example 3: Accessing Dataverse After Authentication

```csharp
// In any tool implementation:
var connection = appState.Connection;
if (connection?.IsReady == true)
{
    // Example: Query all accounts
    var query = new QueryExpression("account");
    query.ColumnSet = new ColumnSet("name", "revenue");
    var results = connection.RetrieveMultiple(query);
    
    foreach (var entity in results.Entities)
    {
        var name = entity["name"];
        // Process...
    }
}
```

## File Structure

```
Watt/Core/Authentication/
├── AuthenticationMethod.cs          # Enum for auth types
├── EnvironmentDetails.cs            # Environment configuration
├── Credentials.cs                   # Base credential classes
├── IAuthenticationProvider.cs        # Authentication interface
├── OAuthAuthenticationProvider.cs    # OAuth implementation
├── ClientSecretAuthenticationProvider.cs  # Service principal
├── UsernamePasswordAuthenticationProvider.cs  # Username/password
├── AuthenticationService.cs          # Main authentication service
├── CredentialManager.cs              # Secure credential storage
├── DataverseConnectionManager.cs     # Connection management
└── README.md                         # Full documentation

Watt/UI/
├── EnvironmentSelectorDialog.cs      # Environment selection UI
├── AddEnvironmentDialog.cs           # Add environment UI
└── AuthenticationDialog.cs           # Authentication UI
```

## Integration Points

### In Your Tools

Access the active connection from AppState:

```csharp
public class MyToolView : IToolView
{
    public View CreateView(AppState state)
    {
        // Check if we have an active connection
        if (state.Connection?.IsReady != true)
        {
            return new Label("No active connection. Select an environment first.");
        }

        // Use the connection
        var query = new QueryExpression("contact");
        var results = state.Connection.RetrieveMultiple(query);
        
        // Build UI with results...
        return myView;
    }
}
```

### Switch Environments

```csharp
// Allow user to switch environments programmatically
bool success = await appState.SwitchEnvironmentAsync(environmentId);
if (success)
{
    // Connection is now active
    // Refresh current tool view
}
```

## Important Setup Steps

### 1. Register Azure AD Application (OAuth only)

If using OAuth authentication:

1. Go to https://portal.azure.com
2. Azure Active Directory → App registrations → New registration
3. Name: "Watt"
4. Redirect URI: `http://localhost`
5. Grant permissions:
   - Microsoft Graph (if needed)
   - Dataverse/PowerApps
6. Copy Application ID → update in OAuthAuthenticationProvider.cs
7. Create client secret (if using app permissions)

### 2. Create Service Principal (Client Secret only)

If using Client Secret authentication:

1. Go to https://portal.azure.com
2. Create service principal (same as Azure AD app)
3. Create client secret
4. Add service principal to Dataverse environment:
   - Power Platform Admin Center
   - Environments → (select your environment)
   - Settings → Users + permissions → Application users
   - New app user → select your service principal
5. Assign security role (e.g., System Administrator)
6. Note: Tenant ID, Client ID, Client Secret

### 3. Prepare Credentials (Username/Password only)

1. Use any organizational account with Dataverse access
2. Username: user@yourorg.onmicrosoft.com or user@company.com
3. Password: User's password

## Key Configuration

Only one file requires configuration:

**File:** `Core/Authentication/OAuthAuthenticationProvider.cs`

```csharp
private const string ClientId = "51f81489-12ee-4a9e-aaae-a2591f45987d"; // ← UPDATE THIS
private const string TenantId = "organizations"; // Multi-tenant, OK as-is
private const string Scopes = "https://org.crm.dynamics.com/.default"; // OK as-is
```

Replace the ClientId with your registered Azure AD application ID.

All other components work out-of-the-box.

## Credential Storage Location

Credentials are stored securely at:
```
%APPDATA%\Watt\
├── watt_credentials.json    (encrypted with DPAPI)
└── watt_environments.json   (JSON configuration)
```

These are created automatically on first use. No manual setup needed.

## Troubleshooting Checklist

- [ ] OAuth ClientId is correct (if using OAuth)
- [ ] Environment URL is in format: https://org.crm.dynamics.com
- [ ] User/service principal has environment access
- [ ] Network connectivity is available
- [ ] Windows user has write access to %APPDATA%
- [ ] Azure AD app has correct redirect URI for OAuth
- [ ] Credentials were not corrupted (delete files to reset)

## Next Steps

1. ✅ Update OAuth ClientId (if needed)
2. ✅ Run `dotnet build`
3. ✅ Run `dotnet run`
4. ✅ Test authentication in the UI
5. ✅ Modify tools to use `appState.Connection`
6. ✅ Deploy with confidence!

## Support

For issues:
1. Check the `README.md` in the Authentication folder
2. Verify all setup steps above
3. Check application logs in debug output
4. Validate Azure AD/service principal configuration

