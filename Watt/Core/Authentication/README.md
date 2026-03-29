# Watt Authentication System

## Overview

The authentication system in Watt provides secure, multi-method authentication to Dataverse environments. Users can authenticate using OAuth (interactive), Client Secret (service principal), or Username/Password credentials.

## Features

- **Multiple Authentication Methods**
  - OAuth: Interactive login for personal and work accounts
  - Client Secret: Service principal authentication (non-interactive)
  - Username/Password: Direct credential authentication

- **Secure Credential Storage**
  - Encrypted storage using Windows DPAPI
  - Per-user encryption (credentials not transferable between user accounts)
  - Automatic token expiration tracking

- **Multiple Environment Support**
  - Store credentials for multiple Dataverse organizations
  - Switch between environments without re-authenticating
  - Credential validation with automatic refresh

- **Connection Management**
  - Cached Dataverse connections
  - Automatic connection pooling
  - On-demand connection establishment

## Architecture

### Core Components

#### `AuthenticationMethod` Enum
Defines supported authentication types:
- `OAuth`: Interactive login
- `ClientSecret`: Service principal
- `UsernamePassword`: Username/Password

#### `EnvironmentDetails`
Represents a Dataverse environment configuration:
```csharp
public class EnvironmentDetails
{
    public string Id { get; set; }              // Unique ID
    public string Name { get; set; }            // User-friendly name
    public string OrgUrl { get; set; }          // https://org.crm.dynamics.com
    public AuthenticationMethod AuthMethod { get; set; }
    public DateTime? LastAuthenticatedAt { get; set; }
    public bool IsAuthenticated { get; set; }
}
```

#### `Credentials` Base Class
Abstract base for credential types:
- `OAuthCredentials`: OAuth tokens
- `ClientSecretCredentials`: Service principal credentials
- `UsernamePasswordCredentials`: Direct credentials

#### `IAuthenticationProvider`
Interface for authentication implementations:
```csharp
public interface IAuthenticationProvider
{
    AuthenticationMethod Method { get; }
    Task<AuthenticationResult> AuthenticateAsync(EnvironmentDetails environment);
    Task<bool> ValidateAsync(Credentials credentials, EnvironmentDetails environment);
}
```

#### `AuthenticationService`
Orchestrates authentication across providers:
- Manages multiple environments
- Handles credential storage
- Validates and refreshes credentials

```csharp
var authService = new AuthenticationService();
await authService.InitializeAsync();

// Register environment
var environment = new EnvironmentDetails
{
    Id = Guid.NewGuid().ToString(),
    Name = "Production",
    OrgUrl = "https://myorg.crm.dynamics.com",
    AuthMethod = AuthenticationMethod.OAuth
};
await authService.RegisterEnvironmentAsync(environment);

// Authenticate
var result = await authService.AuthenticateWithOAuthAsync(environment);
if (result.IsSuccessful)
{
    // Authenticated successfully
}
```

#### `DataverseConnectionManager`
Manages ServiceClient connections:
- Caches connections
- Creates connections on-demand
- Validates connection status

```csharp
var connectionMgr = new DataverseConnectionManager(authService);

// Get connection
var connection = await connectionMgr.GetConnectionAsync(environmentId);
if (connection?.IsReady == true)
{
    // Use connection for Dataverse operations
}
```

#### `CredentialManager`
Handles secure credential storage:
- Encrypts credentials using Windows DPAPI
- Persists to user's AppData folder
- Loads credentials on startup

Stored in: `%APPDATA%\Watt\`
- `watt_credentials.json` (encrypted)
- `watt_environments.json` (plain JSON)

### UI Components

#### `EnvironmentSelectorDialog`
Terminal.Gui dialog for selecting/managing environments:
- View all registered environments
- Add new environments
- Delete environments
- Connect to selected environment
- Shows authentication status (✓/✗)

#### `AddEnvironmentDialog`
Terminal.Gui dialog for registering new environment:
- Enter environment name
- Enter organization URL
- Select authentication method
- Proceeds to authentication

#### `AuthenticationDialog`
Terminal.Gui dialog for authenticating:
- Dynamic UI based on authentication method
- OAuth: Click button for interactive login
- Client Secret: Textfields for tenant ID, client ID, secret
- Username/Password: Textfields for credentials
- Status updates during authentication

## Usage

### Basic Setup

1. **Initialize Services in Application Startup**

```csharp
var authService = new AuthenticationService();
await authService.InitializeAsync();

var connectionManager = new DataverseConnectionManager(authService);

var appState = new AppState
{
    AuthenticationService = authService,
    ConnectionManager = connectionManager
};
```

2. **User Adds Environment**

Users click "Select Environment" → "Add" in the UI, then:
- Enter environment name
- Enter organization URL
- Select authentication method
- Complete authentication flow

3. **User Connects to Environment**

```csharp
bool connected = await appState.SwitchEnvironmentAsync(environmentId);
if (connected && appState.Connection?.IsReady == true)
{
    // Ready to use Dataverse
}
```

### OAuth Authentication

```csharp
var environment = new EnvironmentDetails
{
    Id = Guid.NewGuid().ToString(),
    Name = "Dev Environment",
    OrgUrl = "https://dev.crm.dynamics.com",
    AuthMethod = AuthenticationMethod.OAuth
};

var result = await authService.AuthenticateWithOAuthAsync(environment);
```

**Note:** Update the `ClientId` in `OAuthAuthenticationProvider.cs` with your registered Azure AD application ID.

### Client Secret Authentication

```csharp
var environment = new EnvironmentDetails
{
    Id = Guid.NewGuid().ToString(),
    Name = "Service Account",
    OrgUrl = "https://org.crm.dynamics.com",
    AuthMethod = AuthenticationMethod.ClientSecret
};

var credentials = new ClientSecretCredentials
{
    EnvironmentId = environment.Id,
    TenantId = "your-tenant-id",
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret"
};

var result = await authService.AuthenticateWithClientSecretAsync(environment, credentials);
```

### Username/Password Authentication

```csharp
var environment = new EnvironmentDetails
{
    Id = Guid.NewGuid().ToString(),
    Name = "Cloud Environment",
    OrgUrl = "https://cloud.crm.dynamics.com",
    AuthMethod = AuthenticationMethod.UsernamePassword
};

var credentials = new UsernamePasswordCredentials
{
    EnvironmentId = environment.Id,
    Username = "user@org.com",
    Password = "SecurePassword123"
};

var result = await authService.AuthenticateWithUsernamePasswordAsync(environment, credentials);
```

### Using Dataverse Connection

```csharp
// In a tool view
var connection = appState.Connection;
if (connection?.IsReady == true)
{
    // Query Dataverse
    var query = new QueryExpression("account");
    var result = connection.RetrieveMultiple(query);
}
```

## Security Considerations

1. **DPAPI Encryption**
   - Credentials are encrypted using Windows DPAPI at rest
   - Encryption is user-specific (tied to Windows user account)
   - Not suitable for roaming profiles without additional configuration

2. **Token Expiration**
   - OAuth tokens are validated before use
   - Expired tokens trigger re-authentication automatically
   - Refresh tokens are stored for obtaining new access tokens

3. **Secrets Storage**
   - Client secrets and passwords are encrypted before storage
   - Never logged or displayed in UI
   - TextField with `Secret = true` for password fields

4. **Connection Security**
   - All connections use HTTPS
   - TLS certificate validation enabled
   - Connections cached in memory only (not persisted)

## Configuration

### Azure AD Application (for OAuth)

1. Register app in Azure AD
2. Add Dataverse permissions
3. Set redirect URI to `http://localhost`
4. Update `ClientId` in `OAuthAuthenticationProvider.cs`

### Service Principal (for Client Secret)

1. Create service principal in Azure AD
2. Add to Dataverse environment
3. Grant appropriate security role
4. Store credentials securely

## Troubleshooting

### OAuth Authentication Fails
- Verify Azure AD app is registered
- Check organization URL format
- Ensure account has access to environment
- Check with: `OAuthAuthenticationProvider.ClientId`

### Client Secret Authentication Fails
- Verify tenant ID, client ID, and secret
- Ensure service principal has Dataverse access
- Check environment URL is correct
- Verify credentials in portal

### Credentials Not Loading
- Check `%APPDATA%\Watt\` folder exists
- Verify DPAPI encryption not corrupted
- Re-authenticate if files deleted
- Check Windows user permissions

### Connection Fails After Authentication
- Validate credentials using `ValidateCredentialsAsync`
- Check organization URL
- Verify user/service principal has environment access
- Check network connectivity

## Future Enhancements

- Certificate-based authentication
- Managed identity authentication
- Connection string import/export
- Credential rotation automation
- Multi-factor authentication support
- Azure Key Vault integration for production deployments

