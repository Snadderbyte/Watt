using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Watt.Core.Authentication;

/// <summary>
/// Orchestrates authentication across multiple providers and manages credentials.
/// </summary>
public class AuthenticationService : IAsyncDisposable
{
    private readonly CredentialManager _credentialManager;
    private readonly Dictionary<AuthenticationMethod, IAuthenticationProvider> _providers;
    private readonly ClientSecretAuthenticationProvider _clientSecretProvider;
    private readonly UsernamePasswordAuthenticationProvider _usernamePasswordProvider;

    public AuthenticationService()
    {
        _credentialManager = new CredentialManager();
        _clientSecretProvider = new ClientSecretAuthenticationProvider();
        _usernamePasswordProvider = new UsernamePasswordAuthenticationProvider();

        _providers = new Dictionary<AuthenticationMethod, IAuthenticationProvider>
        {
            { AuthenticationMethod.OAuth, new OAuthAuthenticationProvider() },
            { AuthenticationMethod.ClientSecret, _clientSecretProvider },
            { AuthenticationMethod.UsernamePassword, _usernamePasswordProvider }
        };
    }

    /// <summary>
    /// Initializes the service by loading stored credentials and environments.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _credentialManager.LoadStoredEnvironmentsAsync();
        await _credentialManager.LoadStoredCredentialsAsync();
    }

    /// <summary>
    /// Register a new Dataverse environment.
    /// </summary>
    public async Task RegisterEnvironmentAsync(EnvironmentDetails environment)
    {
        await _credentialManager.SaveEnvironmentAsync(environment);
    }

    /// <summary>
    /// Authenticates with OAuth (interactive login).
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateWithOAuthAsync(EnvironmentDetails environment)
    {
        var provider = _providers[AuthenticationMethod.OAuth];
        var result = await provider.AuthenticateAsync(environment);

        if (result.IsSuccessful && result.Credentials != null)
        {
            await _credentialManager.SaveCredentialsAsync(result.Credentials);
            environment.IsAuthenticated = true;
            environment.LastAuthenticatedAt = DateTime.UtcNow;
            await _credentialManager.SaveEnvironmentAsync(environment);
        }

        return result;
    }

    /// <summary>
    /// Authenticates with Client ID and Secret (Service Principal).
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateWithClientSecretAsync(
        EnvironmentDetails environment,
        ClientSecretCredentials credentials)
    {
        var result = await _clientSecretProvider.AuthenticateWithCredentialsAsync(environment, credentials);

        if (result.IsSuccessful && result.Credentials != null)
        {
            // Store the client secret credentials for future use
            await _credentialManager.SaveCredentialsAsync(credentials);
            // Also store the OAuth token obtained
            await _credentialManager.SaveCredentialsAsync(result.Credentials);
            environment.IsAuthenticated = true;
            environment.LastAuthenticatedAt = DateTime.UtcNow;
            await _credentialManager.SaveEnvironmentAsync(environment);
        }

        return result;
    }

    /// <summary>
    /// Authenticates with username and password.
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateWithUsernamePasswordAsync(
        EnvironmentDetails environment,
        UsernamePasswordCredentials credentials)
    {
        var result = await _usernamePasswordProvider.AuthenticateWithCredentialsAsync(environment, credentials);

        if (result.IsSuccessful && result.Credentials != null)
        {
            await _credentialManager.SaveCredentialsAsync(result.Credentials);
            environment.IsAuthenticated = true;
            environment.LastAuthenticatedAt = DateTime.UtcNow;
            await _credentialManager.SaveEnvironmentAsync(environment);
        }

        return result;
    }

    /// <summary>
    /// Validates if stored credentials are still valid.
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string environmentId)
    {
        var environment = _credentialManager.GetEnvironment(environmentId);
        if (environment == null)
            return false;

        var credentials = _credentialManager.GetCredentials(environmentId);
        if (credentials == null)
            return false;

        var provider = _providers[environment.AuthMethod];
        return await provider.ValidateAsync(credentials, environment);
    }

    /// <summary>
    /// Gets stored credentials for an environment.
    /// </summary>
    public Credentials? GetCredentials(string environmentId) => 
        _credentialManager.GetCredentials(environmentId);

    /// <summary>
    /// Gets environment configuration.
    /// </summary>
    public EnvironmentDetails? GetEnvironment(string environmentId) => 
        _credentialManager.GetEnvironment(environmentId);

    /// <summary>
    /// Gets all registered environments.
    /// </summary>
    public IEnumerable<EnvironmentDetails> GetAllEnvironments() => 
        _credentialManager.GetAllEnvironments();

    /// <summary>
    /// Deletes an environment and its credentials.
    /// </summary>
    public async Task DeleteEnvironmentAsync(string environmentId)
    {
        await _credentialManager.DeleteEnvironmentAsync(environmentId);
    }

    public async ValueTask DisposeAsync()
    {
        await _credentialManager.DisposeAsync();
    }
}
